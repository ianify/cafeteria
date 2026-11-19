using CafeteriaApi.Features.Users;

namespace CafeteriaApi.Features.Orders;

public enum OrderType
{
    DineIn,
    Delivery
}

public enum OrderStatus
{
    Received,
    Preparing,
    Ready,
    Delivered,
    Canceled
}

public class Order
{
    public long Id { get; set; }
    public Guid UserId { get; set; }
    public OrderType OrderType { get; set; } = OrderType.DineIn;
    public string? Address { get; set; } // Nullable since DineIn orders don't need an address
    public OrderStatus Status { get; set; } = OrderStatus.Received;
    public long TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}