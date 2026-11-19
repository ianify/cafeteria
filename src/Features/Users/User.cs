namespace CafeteriaApi.Features.Users;

public enum UserRole
{
    Customer,
    Attendant,
    Barista,
    Manager
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Orders.Order> Orders { get; set; } = new List<Orders.Order>();
}