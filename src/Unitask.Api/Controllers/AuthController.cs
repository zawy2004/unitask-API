using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Unitask.Api.Extensions;
using Unitask.Application.Common.Interfaces;
using Unitask.Application.Common.Models;
using Unitask.Application.DTOs.Auth;

namespace Unitask.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
        var result = await _authService.LoginAsync(request);
        if (result is null)
        {
            return Unauthorized();
        }

        return Ok(MapLoginResponse(result));
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
