using CafeteriaApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaApi.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly CafeteriaDbContext _context;

    public ProductsController(CafeteriaDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<DisplayProducts>> Create([FromBody] CreateProductsRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var responseDto = new DisplayProducts(product.Id, product.Name);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, responseDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DisplayProducts>> GetById(int id)
    {
        // Consulta assíncrona com LINQ e EF Core
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new { mensagem = "Product not found." });
        }

        var responseDto = new DisplayProducts(product.Id, product.Name);
        return Ok(responseDto);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DisplayProducts>>> List()
    {
        // Listagem assíncrona com projeção LINQ
        var products = await _context.Products
            .Select(p => new DisplayProducts(p.Id, p.Name))
            .ToListAsync();

        return Ok(products);
    }
}