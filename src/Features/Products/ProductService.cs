using CafeteriaApi.Data;
using CafeteriaApi.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaApi.Features.Products;

public class ProductService(CafeteriaDbContext context)
{
    public async Task<ProductResult<DisplayProduct>> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new { u.Id, u.Role })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return ProductResult<DisplayProduct>.NotFound("User not found.");
        }

        if (user.Role != UserRole.Manager)
        {
            return ProductResult<DisplayProduct>.Forbidden("Only a Manager can create products.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Category = request.Category?.Trim() ?? string.Empty,
            Size = request.Size,
            Price = request.Price,
            IsAvailable = request.IsAvailable,
            CreatedByUserId = request.UserId
        };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        return ProductResult<DisplayProduct>.Success(MapToDisplay(product));
    }

    public async Task<ProductResult<PagedProductsResult<DisplayProduct>>> ListAsync(
        string? category = null,
        bool? isAvailable = null,
        string? orderBy = null,
        string? direction = "asc",
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            return ProductResult<PagedProductsResult<DisplayProduct>>.BadRequest(
                "Page must be greater than or equal to 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return ProductResult<PagedProductsResult<DisplayProduct>>.BadRequest("PageSize must be between 1 and 100.");
        }

        var normalizedSort = orderBy?.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(normalizedSort) && normalizedSort is not ("price" or "name" or "id"))
        {
            return ProductResult<PagedProductsResult<DisplayProduct>>.BadRequest(
                "Invalid 'orderBy' value. Allowed values: 'price', 'name'.");
        }

        var normalizedDirection = direction?.Trim().ToLowerInvariant() ?? "asc";
        if (normalizedDirection is not ("asc" or "desc"))
        {
            return ProductResult<PagedProductsResult<DisplayProduct>>.BadRequest(
                "Invalid 'direction' value. Allowed values: 'asc', 'desc'.");
        }

        var descending = normalizedDirection == "desc";

        var query = context.Products
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            var trimmedCategory = category.Trim();
            query = query.Where(p => p.Category.ToLower() == trimmedCategory.ToLower());
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(p => p.IsAvailable == isAvailable.Value);
        }

        var totalProducts = await query.CountAsync(cancellationToken);
        var totalPages = totalProducts == 0 ? 0 : (int)Math.Ceiling(totalProducts / (double)pageSize);

        IOrderedQueryable<Product> orderedQuery = normalizedSort switch
        {
            "price" => descending
                ? query.OrderByDescending(p => p.Price).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "name" => descending
                ? query.OrderByDescending(p => p.Name).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Name).ThenBy(p => p.Id),
            _ => descending
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id)
        };

        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new DisplayProduct(
                p.Id,
                p.Name,
                p.Category,
                p.Size,
                p.Price,
                p.IsAvailable,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PagedProductsResult<DisplayProduct>(
            Items: items,
            CurrentPage: page,
            TotalPages: totalPages,
            CurrentProducts: items.Count,
            TotalProducts: totalProducts
        );

        return ProductResult<PagedProductsResult<DisplayProduct>>.Success(result);
    }

    public async Task<DisplayProduct?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return product is null ? null : MapToDisplay(product);
    }

    public async Task<DisplayProduct?> UpdateAsync(int id, UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        if (request.Name is not null)
            product.Name = request.Name.Trim();

        if (request.Category is not null)
            product.Category = request.Category.Trim();

        if (request.Size.HasValue)
            product.Size = request.Size.Value;

        if (request.Price.HasValue)
            product.Price = request.Price.Value;

        if (request.IsAvailable.HasValue)
            product.IsAvailable = request.IsAvailable.Value;

        await context.SaveChangesAsync(cancellationToken);

        return MapToDisplay(product);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static DisplayProduct MapToDisplay(Product product) => new(
        product.Id,
        product.Name,
        product.Category,
        product.Size,
        product.Price,
        product.IsAvailable,
        product.CreatedAt
    );
}