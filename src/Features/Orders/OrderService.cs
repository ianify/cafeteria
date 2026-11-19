using CafeteriaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaApi.Features.Orders;

public enum OrderErrorType
{
    NotFound,
    BadRequest,
    Conflict
}

public sealed record OrderResult<T>(
    T? Value = default,
    OrderErrorType? ErrorType = null,
    string? ErrorMessage = null
)
{
    public bool IsSuccess => ErrorType is null;

    public static OrderResult<T> Success(T value) => new(Value: value);

    public static OrderResult<T> NotFound(string message) =>
        new(ErrorType: OrderErrorType.NotFound, ErrorMessage: message);

    public static OrderResult<T> BadRequest(string message) =>
        new(ErrorType: OrderErrorType.BadRequest, ErrorMessage: message);

    public static OrderResult<T> Conflict(string message) =>
        new(ErrorType: OrderErrorType.Conflict, ErrorMessage: message);
}

public class OrderService(CafeteriaDbContext context)
{
    public async Task<OrderResult<DisplayOrder>> CreateAsync(CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userExists = await context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return OrderResult<DisplayOrder>.NotFound("User not found.");
        }

        if (request.OrderType == OrderType.Delivery && string.IsNullOrWhiteSpace(request.Address))
        {
            return OrderResult<DisplayOrder>.BadRequest("Address is required for Delivery orders.");
        }

        if (request.Items.Count == 0)
        {
            return OrderResult<DisplayOrder>.BadRequest("Order must contain at least one item.");
        }

        var requestedProductIds = request.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var productsById = await context.Products
            .AsNoTracking()
            .Where(p => requestedProductIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        foreach (var itemReq in request.Items)
        {
            if (!productsById.TryGetValue(itemReq.ProductId, out var product))
            {
                return OrderResult<DisplayOrder>.NotFound($"Product with ID {itemReq.ProductId} not found.");
            }

            if (!product.IsAvailable)
            {
                return OrderResult<DisplayOrder>.BadRequest($"Product '{product.Name}' is currently unavailable.");
            }
        }

        var orderItems = request.Items.Select(itemReq =>
        {
            var product = productsById[itemReq.ProductId];
            return new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemReq.Quantity,
                UnitPrice = product.Price,
                Observation = itemReq.Observation?.Trim() ?? string.Empty
            };
        }).ToList();

        var totalAmount = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity);
        var normalizedAddress = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        // 10-second cooldown to avoid accidental duplicate orders
        var windowStart = DateTime.UtcNow.AddSeconds(-10);

        var recentOrders = await context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Where(o =>
                o.UserId == request.UserId &&
                o.CreatedAt >= windowStart &&
                o.Status != OrderStatus.Canceled &&
                o.OrderType == request.OrderType &&
                o.Address == normalizedAddress &&
                o.TotalAmount == totalAmount)
            .ToListAsync(cancellationToken);

        var incomingSignature = orderItems
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .OrderBy(x => x.ProductId)
            .ToList();

        var isDuplicate = recentOrders.Any(existing =>
        {
            var existingSignature = existing.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .OrderBy(x => x.ProductId)
                .ToList();

            return existingSignature.Count == incomingSignature.Count &&
                   existingSignature.Zip(incomingSignature,
                           (a, b) => a.ProductId == b.ProductId && a.Quantity == b.Quantity)
                       .All(match => match);
        });

        if (isDuplicate)
        {
            return OrderResult<DisplayOrder>.Conflict(
                "A duplicate order was just submitted. Please wait a moment before ordering again.");
        }

        var order = new Order
        {
            UserId = request.UserId,
            OrderType = request.OrderType,
            Address = normalizedAddress,
            Status = OrderStatus.Received,
            TotalAmount = totalAmount,
            Items = orderItems
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        var responseItems = order.Items.Select(oi =>
        {
            var product = productsById[oi.ProductId];
            return new DisplayOrderItem(
                Id: oi.Id,
                ProductId: oi.ProductId,
                ProductName: product.Name,
                Quantity: oi.Quantity,
                UnitPrice: oi.UnitPrice,
                Subtotal: oi.UnitPrice * oi.Quantity,
                Observation: oi.Observation
            );
        }).ToList();

        var displayOrder = new DisplayOrder(
            Id: order.Id,
            UserId: order.UserId,
            OrderType: order.OrderType,
            Address: order.Address,
            Status: order.Status,
            TotalAmount: order.TotalAmount,
            CreatedAt: order.CreatedAt,
            Items: responseItems
        );

        return OrderResult<DisplayOrder>.Success(displayOrder);
    }

    public async Task<OrderResult<PagedOrdersResult<DisplayOrder>>> ListAsync(
        OrderStatus? status = null,
        Guid? userId = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            return OrderResult<PagedOrdersResult<DisplayOrder>>.BadRequest("Invalid order status filter.");
        }

        if (page < 1)
        {
            return OrderResult<PagedOrdersResult<DisplayOrder>>.BadRequest("Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return OrderResult<PagedOrdersResult<DisplayOrder>>.BadRequest("PageSize must be between 1 and 100.");
        }

        var query = context.Orders
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        var totalOrders = await query.CountAsync(cancellationToken);
        var totalPages = totalOrders == 0 ? 0 : (int)Math.Ceiling(totalOrders / (double)pageSize);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new DisplayOrder(
                o.Id,
                o.UserId,
                o.OrderType,
                o.Address,
                o.Status,
                o.TotalAmount,
                o.CreatedAt,
                o.Items.Select(i => new DisplayOrderItem(
                    i.Id,
                    i.ProductId,
                    i.Product != null ? i.Product.Name : string.Empty,
                    i.Quantity,
                    i.UnitPrice,
                    i.UnitPrice * i.Quantity,
                    i.Observation
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        var result = new PagedOrdersResult<DisplayOrder>(
            Items: items,
            CurrentPage: page,
            TotalPages: totalPages,
            CurrentOrders: items.Count,
            TotalOrders: totalOrders
        );

        return OrderResult<PagedOrdersResult<DisplayOrder>>.Success(result);
    }

    public async Task<DisplayOrder?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(o => new DisplayOrder(
                o.Id,
                o.UserId,
                o.OrderType,
                o.Address,
                o.Status,
                o.TotalAmount,
                o.CreatedAt,
                o.Items.Select(i => new DisplayOrderItem(
                    i.Id,
                    i.ProductId,
                    i.Product != null ? i.Product.Name : string.Empty,
                    i.Quantity,
                    i.UnitPrice,
                    i.UnitPrice * i.Quantity,
                    i.Observation
                )).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<OrderResult<DisplayOrder>> UpdateStatusAsync(
        long id,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Status.HasValue || !Enum.IsDefined(request.Status.Value))
        {
            return OrderResult<DisplayOrder>.BadRequest("Invalid order status value.");
        }

        var order = await context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
        {
            return OrderResult<DisplayOrder>.NotFound("Order not found.");
        }

        var targetStatus = request.Status.Value;

        if (targetStatus == OrderStatus.Canceled && order.Status != OrderStatus.Received)
        {
            return OrderResult<DisplayOrder>.BadRequest(
                $"Order can only be canceled when its current status is '{OrderStatus.Received}' (current: '{order.Status}').");
        }

        order.Status = targetStatus;
        await context.SaveChangesAsync(cancellationToken);

        return OrderResult<DisplayOrder>.Success(MapToDisplay(order));
    }

    private static DisplayOrder MapToDisplay(Order order) => new(
        Id: order.Id,
        UserId: order.UserId,
        OrderType: order.OrderType,
        Address: order.Address,
        Status: order.Status,
        TotalAmount: order.TotalAmount,
        CreatedAt: order.CreatedAt,
        Items: order.Items.Select(i => new DisplayOrderItem(
            Id: i.Id,
            ProductId: i.ProductId,
            ProductName: i.Product?.Name ?? string.Empty,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            Subtotal: i.UnitPrice * i.Quantity,
            Observation: i.Observation
        )).ToList()
    );
}