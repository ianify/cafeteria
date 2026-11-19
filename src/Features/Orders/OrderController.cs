using Microsoft.AspNetCore.Mvc;

namespace CafeteriaApi.Features.Orders;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrderService orderService) : ControllerBase
{
    // CREATE ORDER FROM PRODUCTS
    [HttpPost]
    public async Task<ActionResult<DisplayOrder>> Create([FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        var created = result.Value!;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

// SELECT ORDERS (WITH OPTIONAL FILTERS & PAGINATION)
    [HttpGet]
    public async Task<ActionResult<PagedOrdersResult<DisplayOrder>>> List(
        [FromQuery] OrderStatus? status,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await orderService.ListAsync(status, userId, page, pageSize, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Value);
    }

    // SELECT ORDER BY ID
    [HttpGet("{id:long}")]
    public async Task<ActionResult<DisplayOrder>> GetById(long id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound(new { message = "Order not found." });
        }

        return Ok(order);
    }

    // UPDATE ORDER STATUS ONLY
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<DisplayOrder>> UpdateStatus(
        long id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.UpdateStatusAsync(id, request, cancellationToken);
        if (!result.IsSuccess)
        {
            return ToErrorResult(result);
        }

        return Ok(result.Value);
    }

    private ActionResult ToErrorResult<T>(OrderResult<T> result) => result.ErrorType switch
    {
        OrderErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
        OrderErrorType.Conflict => Conflict(new { message = result.ErrorMessage }),
        _ => BadRequest(new { message = result.ErrorMessage })
    };
}