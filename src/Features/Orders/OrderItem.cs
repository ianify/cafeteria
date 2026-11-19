namespace CafeteriaApi.Features.Orders;

public class OrderItem
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public long UnitPrice { get; set; }
    public string Observation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Adicionado

    // Propriedades de navegação
    public Order? Order { get; set; }
    public Products.Product? Product { get; set; }
}