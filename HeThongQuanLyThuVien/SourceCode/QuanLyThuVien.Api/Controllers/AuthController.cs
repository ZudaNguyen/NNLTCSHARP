using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken ct)
    {
        try { return Ok(await authService.LoginAsync(request, ct)); }
        catch (InvalidLoginException ex) { return Unauthorized(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status401Unauthorized }); }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthUserDto>> Me(CancellationToken ct)
    {
        if (!TryGetAccountId(out var accountId)) return Unauthorized();
        var user = await authService.GetMeAsync(accountId, ct);
        return user is null ? Unauthorized() : Ok(user);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (!TryGetAccountId(out var accountId) || !Guid.TryParse(User.FindFirstValue("session_id"), out var sessionId))
            return Unauthorized();
        await authService.LogoutAsync(accountId, sessionId, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken ct)
    {
        if (!TryGetAccountId(out var accountId)) return Unauthorized();
        try
        {
            await authService.ChangePasswordAsync(accountId, request, ct);
            return NoContent();
        }
        catch (InvalidLoginException)
        {
            return BadRequest(new ProblemDetails { Title = "Mật khẩu hiện tại không đúng.", Status = StatusCodes.Status400BadRequest });
        }
        catch (PasswordPolicyException ex)
        {
            return BadRequest(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    private bool TryGetAccountId(out int accountId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out accountId) && accountId > 0;
}
