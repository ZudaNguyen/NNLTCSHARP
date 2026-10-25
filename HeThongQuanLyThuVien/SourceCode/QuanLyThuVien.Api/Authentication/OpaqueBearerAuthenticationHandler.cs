using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using QuanLyThuVien.Application.Interfaces;

namespace QuanLyThuVien.Api.Authentication;

public static class LibraryAuthenticationDefaults
{
    public const string Scheme = "LibrarySession";
}

public sealed class OpaqueBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthService authService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header)) return AuthenticateResult.NoResult();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Unsupported authorization scheme.");

        var token = header[7..].Trim();
        if (token.Length < 32) return AuthenticateResult.Fail("Invalid bearer token.");
        var principal = await authService.ValidateSessionAsync(token, Context.RequestAborted);
        if (principal is null) return AuthenticateResult.Fail("Invalid or expired bearer token.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, principal.AccountId.ToString()),
            new(ClaimTypes.Name, principal.UserName),
            new(ClaimTypes.GivenName, principal.DisplayName),
            new(ClaimTypes.Role, principal.Role),
            new("session_id", principal.SessionId.ToString("D"))
        };
        if (principal.EmployeeId.HasValue) claims.Add(new Claim("employee_id", principal.EmployeeId.Value.ToString()));
        if (principal.ReaderId.HasValue) claims.Add(new Claim("reader_id", principal.ReaderId.Value.ToString()));
        var identity = new ClaimsIdentity(claims, LibraryAuthenticationDefaults.Scheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), LibraryAuthenticationDefaults.Scheme);
        return AuthenticateResult.Success(ticket);
    }
}
