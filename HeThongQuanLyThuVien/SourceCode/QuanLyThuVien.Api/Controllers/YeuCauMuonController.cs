using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/yeucau")]
public sealed class YeuCauMuonController(IYeuCauMuonService service) : ControllerBase
{
    private bool TryGetReaderId(out int readerId) =>
        int.TryParse(User.FindFirstValue("reader_id"), out readerId) && readerId > 0;

    private string ApproverName =>
        User.FindFirstValue(ClaimTypes.GivenName)
        ?? User.Identity?.Name
        ?? "Thủ thư";

    // ------------------------------ Độc giả ------------------------------

    [HttpPost]
    [Authorize(Roles = AccountRoles.Reader)]
    public async Task<IActionResult> Create([FromBody] TaoYeuCauMuonDto request, CancellationToken ct)
    {
        if (!TryGetReaderId(out var readerId)) return Forbid();
        try
        {
            var result = await service.CreateAsync(readerId, request, ct);
            return CreatedAtAction(nameof(MyRequest), new { id = result.MaYeuCau }, result);
        }
        catch (BorrowingRuleException ex) { return BadRequest(new { message = ex.Message }); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Conflict(new { message = "Dữ liệu đã thay đổi hoặc vi phạm ràng buộc cơ sở dữ liệu." });
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = AccountRoles.Reader)]
    public async Task<IActionResult> MyRequests([FromQuery] string? trangThai, CancellationToken ct)
    {
        if (!TryGetReaderId(out var readerId)) return Forbid();
        return Ok(await service.GetForReaderAsync(readerId, trangThai, ct));
    }

    [HttpGet("me/{id:int}")]
    [Authorize(Roles = AccountRoles.Reader)]
    public async Task<IActionResult> MyRequest(int id, CancellationToken ct)
    {
        if (!TryGetReaderId(out var readerId)) return Forbid();
        var result = await service.GetByIdForReaderAsync(readerId, id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("me/{id:int}/huy")]
    [Authorize(Roles = AccountRoles.Reader)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        if (!TryGetReaderId(out var readerId)) return Forbid();
        try
        {
            var result = await service.CancelAsync(readerId, id, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BorrowingRuleException ex) { return BadRequest(new { message = ex.Message }); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
    }

    // ------------------------------ Thủ thư ------------------------------

    [HttpGet]
    [Authorize(Roles = AccountRoles.Librarian)]
    public async Task<IActionResult> Search(
        [FromQuery] string? search, [FromQuery] string? trangThai, CancellationToken ct) =>
        Ok(await service.SearchAsync(search, trangThai, ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = AccountRoles.Librarian)]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:int}/duyet")]
    [Authorize(Roles = AccountRoles.Librarian)]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        try
        {
            var result = await service.ApproveAsync(id, ApproverName, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BorrowingRuleException ex) { return BadRequest(new { message = ex.Message }); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Conflict(new { message = "Dữ liệu đã thay đổi hoặc vi phạm ràng buộc cơ sở dữ liệu." });
        }
    }

    [HttpPost("{id:int}/tu-choi")]
    [Authorize(Roles = AccountRoles.Librarian)]
    public async Task<IActionResult> Reject(
        int id, [FromBody] TuChoiYeuCauMuonDto request, CancellationToken ct)
    {
        try
        {
            var result = await service.RejectAsync(id, ApproverName, request.LyDo, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BorrowingRuleException ex) { return BadRequest(new { message = ex.Message }); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
    }
}
