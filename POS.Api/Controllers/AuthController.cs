using BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS.Api.Auth;

namespace POS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password);

        if (!result.IsSuccess || result.Value is null)
            return Unauthorized(new { message = result.Error });

        var user = result.Value;
        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = await _authService.IssueRefreshTokenAsync(user.UserId);

        return Ok(new LoginResponse(token, refreshToken, user.Username, user.RoleName, user.FullName));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        var result = await _authService.RefreshAsync(request.RefreshToken);

        if (!result.IsSuccess || result.Value is null)
            return Unauthorized(new { message = result.Error });

        var token = _jwtTokenService.GenerateToken(result.Value);

        return Ok(new RefreshResponse(token));
    }
}