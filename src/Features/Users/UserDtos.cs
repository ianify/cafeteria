using System.ComponentModel.DataAnnotations;

namespace CafeteriaApi.Features.Users;

public record CreateUserRequest(
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 30 characters.")]
    string Username,

    UserRole Role = UserRole.Customer
);

public record DisplayUser(
    Guid Id,
    string Username,
    UserRole Role,
    DateTime CreatedAt
);