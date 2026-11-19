using CafeteriaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CafeteriaApi.Features.Users;

public class UserService(CafeteriaDbContext context)
{
    public async Task<DisplayUser> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Username = request.Username.Trim(),
            Role = request.Role
        };

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return MapToDisplay(user);
    }

    public async Task<DisplayUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user is null ? null : MapToDisplay(user);
    }

    private static DisplayUser MapToDisplay(User user) => new(
        user.Id,
        user.Username,
        user.Role,
        user.CreatedAt
    );
}