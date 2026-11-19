using CafeteriaApi.Features.Users;

namespace CafeteriaApi.Features.Products;

public enum ProductSize
{
    P,
    M,
    G
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ProductSize Size { get; set; }
    public long Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}