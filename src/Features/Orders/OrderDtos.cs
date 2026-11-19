using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.Features.Orders;

public record CreateOrderItemRequest(
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
    int ProductId,
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
    int Quantity,
    [StringLength(200, ErrorMessage = "Observation must be at most 200 characters.")]
    string? Observation = null
);

public record CreateOrderRequest(
    [Required(ErrorMessage = "UserId is required.")]
    Guid UserId,
    OrderType OrderType = OrderType.DineIn,
    [StringLength(200, ErrorMessage = "Address must be at most 200 characters.")]
    string? Address = null,
    [Required(ErrorMessage = "At least one item is required.")]
    [MinLength(1, ErrorMessage = "At least one item is required.")]
    List<CreateOrderItemRequest> Items = null!
);

public record DisplayOrderItem(
    long Id,
    int ProductId,
    string ProductName,
    int Quantity,
    long UnitPrice,
    long Subtotal,
    string Observation
);

public record DisplayOrder(
    long Id,
    Guid UserId,
    OrderType OrderType,
    string? Address,
    OrderStatus Status,
    long TotalAmount,
    DateTime CreatedAt,
    List<DisplayOrderItem> Items
);

public record PagedOrdersResult<T>(
    IReadOnlyList<T> Items,
    int CurrentPage,
    int TotalPages,
    int CurrentOrders,
    int TotalOrders
);
public record UpdateOrderStatusRequest(
    [Required(ErrorMessage = "Status is required.")]
    OrderStatus? Status
);