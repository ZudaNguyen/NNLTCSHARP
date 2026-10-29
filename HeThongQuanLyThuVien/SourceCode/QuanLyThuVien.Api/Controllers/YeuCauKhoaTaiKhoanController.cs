using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Api.Controllers;

/// <summary>
/// Yêu cầu khóa tài khoản độc giả: thủ thư gửi/hủy, Admin duyệt/từ chối.
/// Thủ thư không có quyền khóa tài khoản trực tiếp.
/// </summary>
[ApiController]
[Route("api/yeucau-khoa")]
[Authorize(Roles = AccountRoles.Staff)]
public sealed class YeuCauKhoaTaiKhoanController(IYeuCauKhoaTaiKhoanService service) : ControllerBase
{
    private bool IsAdmin => User.IsInRole(AccountRoles.Admin);

    private bool TryGetEmployeeId(out int employeeId) =>
        int.TryParse(User.FindFirstValue("employee_id"), out employeeId) && employeeId > 0;

    private string ApproverName =>
        User.FindFirstValue(ClaimTypes.GivenName)
        ?? User.Identity?.Name
        ?? "Admin";

    /// <summary>Phạm vi xem: Admin xem tất cả, thủ thư chỉ xem yêu cầu của mình.</summary>
    private int? ScopeEmployeeId => IsAdmin ? null : TryGetEmployeeId(out var id) ? id : -1;

    [HttpGet("tai-khoan")]
    public async Task<IActionResult> ReaderAccounts([FromQuery] string? search, CancellationToken ct) =>
        Ok(await service.FindReaderAccountsAsync(search, ct));

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string? trangThai, [FromQuery] string? search, CancellationToken ct) =>
        Ok(await service.SearchAsync(ScopeEmployeeId, trangThai, search, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ScopeEmployeeId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = AccountRoles.Librarian)]
    public async Task<IActionResult> Create([FromBody] TaoYeuCauKhoaTaiKhoanDto request, CancellationToken ct)
    {
        if (!TryGetEmployeeId(out var employeeId)) return Forbid();
        try
        {
            var result = await service.CreateAsync(employeeId, request, ct);
            return CreatedAtAction(nameof(Get), new { id = result.MaYeuCau }, result);
        }
        catch (AccountLockRequestException ex) { return BadRequest(new { message = ex.Message }); }
        catch (AccountLockRequestConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Dữ liệu đã thay đổi hoặc vi phạm ràng buộc cơ sở dữ liệu." });
        }
    }

    [HttpPost("{id:int}/huy")]
    [Authorize(Roles = AccountRoles.Librarian)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        if (!TryGetEmployeeId(out var employeeId)) return Forbid();
        try
        {
            var result = await service.CancelAsync(employeeId, id, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (AccountLockRequestException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/duyet")]
    [Authorize(Roles = AccountRoles.Admin)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        try
        {
            var result = await service.ApproveAsync(id, ApproverName, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (AccountLockRequestException ex) { return BadRequest(new { message = ex.Message }); }
        catch (AccountLockRequestConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Dữ liệu đã thay đổi hoặc vi phạm ràng buộc cơ sở dữ liệu." });
        }
    }

    [HttpPost("{id:int}/tu-choi")]
    [Authorize(Roles = AccountRoles.Admin)]
    public async Task<IActionResult> Reject(
        int id, [FromBody] TuChoiYeuCauKhoaTaiKhoanDto request, CancellationToken ct)
    {
        try
        {
            var result = await service.RejectAsync(id, ApproverName, request.LyDo, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (AccountLockRequestException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
