namespace CafeteriaApi.Features.Products;

using System.ComponentModel.DataAnnotations;

public enum ProductErrorType
{
    NotFound,
    BadRequest,
    Forbidden
}

public record CreateProductRequest(
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 20 characters.")]
    string Name,
    Guid UserId,
    [StringLength(30, ErrorMessage = "Category must be at most 30 characters.")]
    string? Category = null,
    ProductSize Size = ProductSize.M,
    [Range(0, long.MaxValue, ErrorMessage = "Price cannot be negative.")]
    long Price = 0,
    bool IsAvailable = true
);

public record UpdateProductRequest(
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 20 characters.")]
    string? Name = null,
    [StringLength(30, ErrorMessage = "Category must be at most 30 characters.")]
    string? Category = null,
    ProductSize? Size = null,
    [Range(0, long.MaxValue, ErrorMessage = "Price cannot be negative.")]
    long? Price = null,
    bool? IsAvailable = null
);

public record DisplayProduct(
    int Id,
    string Name,
    string Category,
    ProductSize Size,
    long Price,
    bool IsAvailable,
    DateTime CreatedAt
);

public record PagedProductsResult<T>(
    IReadOnlyList<T> Items,
    int CurrentPage,
    int TotalPages,
    int CurrentProducts,
    int TotalProducts
);

public sealed record ProductResult<T>(
    T? Value = default,
    ProductErrorType? ErrorType = null,
    string? ErrorMessage = null
)
{
    public bool IsSuccess => ErrorType is null;

    public static ProductResult<T> Success(T value) => new(Value: value);

    public static ProductResult<T> NotFound(string message) =>
        new(ErrorType: ProductErrorType.NotFound, ErrorMessage: message);

    public static ProductResult<T> BadRequest(string message) =>
        new(ErrorType: ProductErrorType.BadRequest, ErrorMessage: message);

    public static ProductResult<T> Forbidden(string message) =>
        new(ErrorType: ProductErrorType.Forbidden, ErrorMessage: message);
}