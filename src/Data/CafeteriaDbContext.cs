using CafeteriaApi.Features.Orders;
using CafeteriaApi.Features.Products;
using CafeteriaApi.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaApi.Data;

public class CafeteriaDbContext(DbContextOptions<CafeteriaDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Product>()
            .Property(p => p.Size)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderType)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();
    }
}