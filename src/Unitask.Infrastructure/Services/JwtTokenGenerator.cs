using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Settings;
using Unitask.Domain.Entities;

namespace Unitask.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateAccessToken(User user)
    {
        return GenerateToken(user, TimeSpan.FromMinutes(_jwtSettings.ExpiryMinutes), "access");
    }

    public string GenerateRefreshToken(User user)
    {
        var refreshDays = _jwtSettings.RefreshExpiryDays > 0 ? _jwtSettings.RefreshExpiryDays : 7;
        return GenerateToken(user, TimeSpan.FromDays(refreshDays), "refresh");
    }

    private string GenerateToken(User user, TimeSpan expiresIn, string tokenType)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.UserType),
            new("fullName", user.FullName),
            new("typ", tokenType)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(expiresIn),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
