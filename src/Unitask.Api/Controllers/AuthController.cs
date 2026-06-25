using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Unitask.Api.Extensions;
using Unitask.Api.Services;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Models;
using Unitask.Application.DTOs.Auth;
using Unitask.Infrastructure.Persistence;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly UnitaskDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthController(
        IAuthService authService,
        IJwtTokenGenerator jwtTokenGenerator,
        UnitaskDbContext dbContext,
        IConfiguration configuration)
    {
        _authService = authService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(MapRegisterResponse(result));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
            {
                var demoUser = FallbackData.TryGetDemoUser(request.Email, request.Password);
                return demoUser is null ? Unauthorized() : Ok(MapLoginResponse(FallbackData.CreateAuthResult(demoUser, _jwtTokenGenerator)));
            }

            return Ok(MapLoginResponse(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            // Tài khoản bị vô hiệu hóa — không fallback sang demo.
            return StatusCode(403, new { message = ex.Message });
        }
        catch
        {
            var demoUser = FallbackData.TryGetDemoUser(request.Email, request.Password);
            return demoUser is null ? Unauthorized() : Ok(MapLoginResponse(FallbackData.CreateAuthResult(demoUser, _jwtTokenGenerator)));
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(new RefreshTokenResponse
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await _authService.LogoutAsync(userId.Value);
        return Ok();
    }

    [HttpPost("google")]
    public async Task<ActionResult<LoginResponse>> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
        try
        {
            var clientId = _configuration["GoogleAuth:ClientId"]
                ?? "308973806649-koiqv3ta5iv4fdgvlj5ckc7kvarot7sq.apps.googleusercontent.com";

            var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };
            payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch
        {
            return Unauthorized(new { message = "Google token không hợp lệ." });
        }

        var email = payload.Email;
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Không lấy được email từ Google." });

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            user = new Unitask.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                FullName = payload.Name ?? email.Split('@')[0],
                UserType = "student",
                AvatarUrl = payload.Picture,
                IsActive = true,
                IsVerified = payload.EmailVerified,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        var token = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user);

        return Ok(new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            User = new AuthUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                UserType = user.UserType
            }
        });
    }

    private static RegisterResponse MapRegisterResponse(AuthResult result)
    {
        return new RegisterResponse
        {
            Id = result.User.Id,
            Email = result.User.Email,
            FullName = result.User.FullName,
            UserType = result.User.UserType,
            Token = result.Token,
            RefreshToken = result.RefreshToken
        };
    }

    private static LoginResponse MapLoginResponse(AuthResult result)
    {
        return new LoginResponse
        {
            Token = result.Token,
            RefreshToken = result.RefreshToken,
            User = new AuthUserDto
            {
                Id = result.User.Id,
                Email = result.User.Email,
                FullName = result.User.FullName,
                UserType = result.User.UserType
            }
        };
    }
}
