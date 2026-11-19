using Microsoft.AspNetCore.Mvc;

namespace CafeteriaApi.Features.Products;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService productService) : ControllerBase
{
    // CREATE
    [HttpPost]
    public async Task<ActionResult<DisplayProduct>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await productService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        var created = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    private ActionResult ToErrorResult<T>(ProductResult<T> result) => result.ErrorType switch
    {
        ProductErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
        ProductErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = result.ErrorMessage }),
        _ => BadRequest(new { message = result.ErrorMessage })
    };

// SELECT ALL (WITH OPTIONAL FILTERS, SORTING & PAGINATION)
    [HttpGet]
    public async Task<ActionResult<PagedProductsResult<DisplayProduct>>> List(
        [FromQuery] string? category,
        [FromQuery] bool? isAvailable,
        [FromQuery] string? orderBy,
        [FromQuery] string? direction = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await productService.ListAsync(
            category,
            isAvailable,
            orderBy,
            direction,
            page,
            pageSize,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorType == ProductErrorType.NotFound
                ? NotFound(new { message = result.ErrorMessage })
                : BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    // SELECT BY ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DisplayProduct>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(product);
    }

    // PARTIAL UPDATE
    [HttpPatch("{id:int}")]
    public async Task<ActionResult<DisplayProduct>> Update(int id, [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await productService.UpdateAsync(id, request, cancellationToken);
        if (updated is null)
        {
            return NotFound(new { message = "Product not found." });
        }

        return Ok(updated);
    }

    // DELETE
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = "Product not found." });
        }

        return NoContent();
    }
}