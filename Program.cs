using CafeteriaApi.Data;
using CafeteriaApi.External;
using CafeteriaApi.Features.Orders;
using CafeteriaApi.Features.Products;
using CafeteriaApi.Features.Users;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRO DE SERVIÇOS
builder.Services.AddDbContext<CafeteriaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddHttpClient<ViaCepService>(client =>
{
    client.BaseAddress = new Uri("https://viacep.com.br/ws/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

// 2. MIGRAÇÕES E SEED INICIAL (DESENVOLVIMENTO)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CafeteriaDbContext>();

    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext);
}

// 3. CONFIGURAÇÃO DO PIPELINE HTTP
if (app.Environment.IsDevelopment())
{
    // Gera o documento JSON nativo do .NET 10
    app.MapOpenApi();
    // Habilita a interface visual clássica do Swagger consumindo o JSON nativo
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "CafeteriaApi v1");
        options.RoutePrefix = "swagger"; // Deixa o Swagger acessível na raiz /swagger
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();