using Unitask.Domain.Entities;

namespace Unitask.Application.Common.Models;

public class AuthResult
{
    public User User { get; set; } = null!;

    public string Token { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;
}
