using CafeteriaApi.Features.Products;
using CafeteriaApi.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaApi.Data;

public static class DbSeeder
{
    // Fixed IDs so seeding is idempotent across restarts
    public static readonly Guid SeedManagerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private record SeedCatalogItem(string Name, string Category);

    public static async Task SeedAsync(CafeteriaDbContext context, CancellationToken cancellationToken = default)
    {
        var manager = await context.Users
            .FirstOrDefaultAsync(u => u.Id == SeedManagerId, cancellationToken);

        if (manager is null)
        {
            manager = new User
            {
                Id = SeedManagerId,
                Username = "Ian",
                Role = UserRole.Manager,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(manager);
            await context.SaveChangesAsync(cancellationToken);
        }

        var existingProductCount = await context.Products.CountAsync(cancellationToken);
        if (existingProductCount >= 50)
        {
            return;
        }

        SeedCatalogItem[] catalog =
        [
            // Cafés (10)
            new("Café Coado", "Cafés"),
            new("Café Filtrado", "Cafés"),
            new("Café com Leite", "Cafés"),
            new("Cappuccino", "Cafés"),
            new("Latte", "Cafés"),
            new("Mocha", "Cafés"),
            new("Café Gelado", "Cafés"),
            new("Café Cremoso", "Cafés"),
            new("Café com Canela", "Cafés"),
            new("Café Mocha Gelado", "Cafés"),

            // Expressos (10)
            new("Espresso Curto", "Expressos"),
            new("Espresso Longo", "Expressos"),
            new("Espresso Duplo", "Expressos"),
            new("Americano", "Expressos"),
            new("Macchiato", "Expressos"),
            new("Flat White", "Expressos"),
            new("Cortado", "Expressos"),
            new("Ristretto", "Expressos"),
            new("Espresso Tônica", "Expressos"),
            new("Espresso Chocolate", "Expressos"),

            // Chás (10)
            new("Chá Verde", "Chás"),
            new("Chá Preto", "Chás"),
            new("Chá de Camomila", "Chás"),
            new("Chá de Hortelã", "Chás"),
            new("Chá Gelado", "Chás"),
            new("Chá de Hibisco", "Chás"),
            new("Chá Branco", "Chás"),
            new("Chá Mate Gelado", "Chás"),
            new("Chá de Frutas", "Chás"),
            new("Chá de Erva-Doce", "Chás"),

            // Salgados (10)
            new("Pão de Queijo", "Salgados"),
            new("Coxinha", "Salgados"),
            new("Esfirra de Carne", "Salgados"),
            new("Esfirra de Frango", "Salgados"),
            new("Misto Quente", "Salgados"),
            new("Empada de Frango", "Salgados"),
            new("Enroladinho", "Salgados"),
            new("Croissant Queijo", "Salgados"),
            new("Quiche de Alho", "Salgados"),
            new("Sanduíche Natural", "Salgados"),

            // Sobremesas (10)
            new("Bolo de Cenoura", "Sobremesas"),
            new("Bolo de Chocolate", "Sobremesas"),
            new("Brigadeiro", "Sobremesas"),
            new("Brownie", "Sobremesas"),
            new("Cheesecake", "Sobremesas"),
            new("Torta de Limão", "Sobremesas"),
            new("Pudim de Leite", "Sobremesas"),
            new("Cookie Gotas Choc", "Sobremesas"),
            new("Bolo de Fubá", "Sobremesas"),
            new("Pavê de Chocolate", "Sobremesas")
        ];

        ProductSize[] sizes = [ProductSize.P, ProductSize.M, ProductSize.G];
        var baseTime = DateTime.UtcNow;
        var productsToAdd = new List<Product>(catalog.Length);

        for (int i = 0; i < catalog.Length; i++)
        {
            var item = catalog[i];
            var size = sizes[i % sizes.Length];
            var suffix = $" #{i + 1:00}";
            var maxBaseLength = 20 - suffix.Length;
            var trimmedBaseName = item.Name.Length > maxBaseLength
                ? item.Name[..maxBaseLength].Trim()
                : item.Name;

            var name = $"{trimmedBaseName}{suffix}";

            productsToAdd.Add(new Product
            {
                Name = name,
                Category = item.Category,
                Size = size,
                Price = 800L + ((i + 1) * 150L),
                IsAvailable = (i + 1) % 7 != 0,
                CreatedAt = baseTime.AddMinutes(-50 + (i + 1)),
                CreatedByUserId = SeedManagerId
            });
        }

        context.Products.AddRange(productsToAdd);
        await context.SaveChangesAsync(cancellationToken);
    }
}