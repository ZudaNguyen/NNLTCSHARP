using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/taikhoan")]
[Authorize(Roles = AccountRoles.Admin)]
public sealed class TaiKhoanController(IAccountService accountService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountDto>>> Search(
        [FromQuery] string? search, [FromQuery] string? vaiTro, [FromQuery] string? trangThai, CancellationToken ct) =>
        Ok(await accountService.SearchAsync(search, vaiTro, trangThai, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDto>> GetById(int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest();
        var account = await accountService.GetByIdAsync(id, ct);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountRequestDto request, CancellationToken ct)
    {
        try
        {
            var account = await accountService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = account.MaTaiKhoan }, account);
        }
        catch (AccountConflictException ex) { return Conflict(Problem(StatusCodes.Status409Conflict, ex.Message)); }
        catch (PasswordPolicyException ex) { return BadRequest(Problem(StatusCodes.Status400BadRequest, ex.Message)); }
        catch (DbUpdateException) { return Conflict(Problem(StatusCodes.Status409Conflict, "Tên đăng nhập hoặc hồ sơ đã được liên kết với tài khoản khác.")); }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccountDto>> Update(int id, [FromBody] UpdateAccountRequestDto request, CancellationToken ct)
    {
        if (id <= 0) return BadRequest();
        if (!TryGetActorId(out var actorId)) return Unauthorized();
        try
        {
            var account = await accountService.UpdateAsync(id, request, actorId, ct);
            return account is null ? NotFound() : Ok(account);
        }
        catch (AccountConflictException ex) { return Conflict(Problem(StatusCodes.Status409Conflict, ex.Message)); }
        catch (AccountForbiddenException) { return Forbid(); }
        catch (DbUpdateException) { return Conflict(Problem(StatusCodes.Status409Conflict, "Hồ sơ nhân viên/độc giả đã được liên kết với tài khoản khác.")); }
    }

    [HttpPatch("{id:int}/trang-thai")]
    public async Task<ActionResult<AccountDto>> UpdateStatus(int id, [FromBody] UpdateAccountStatusRequestDto request, CancellationToken ct)
    {
        if (id <= 0) return BadRequest();
        if (!TryGetActorId(out var actorId)) return Unauthorized();
        try
        {
            var account = await accountService.UpdateStatusAsync(id, request.TrangThai, actorId, ct);
            return account is null ? NotFound() : Ok(account);
        }
        catch (AccountConflictException ex) { return Conflict(Problem(StatusCodes.Status409Conflict, ex.Message)); }
        catch (AccountForbiddenException) { return Forbid(); }
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetAccountPasswordRequestDto request, CancellationToken ct)
    {
        if (id <= 0) return BadRequest();
        try
        {
            return await accountService.ResetPasswordAsync(id, request.MatKhauMoi, ct) ? NoContent() : NotFound();
        }
        catch (PasswordPolicyException ex) { return BadRequest(Problem(StatusCodes.Status400BadRequest, ex.Message)); }
    }

    private bool TryGetActorId(out int accountId) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out accountId) && accountId > 0;

    private static ProblemDetails Problem(int status, string title) => new() { Status = status, Title = title };
}
