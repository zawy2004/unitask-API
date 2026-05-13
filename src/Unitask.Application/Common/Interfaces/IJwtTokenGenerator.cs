using Unitask.Domain.Entities;

namespace Unitask.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken(User user);
}
