using Microsoft.AspNetCore.Mvc;

namespace CafeteriaApi.Features.Users;

[ApiController]
[Route("api/[controller]")]
public class UserController(UserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<DisplayUser>> Create([FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var created = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DisplayUser>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(user);
    }
}