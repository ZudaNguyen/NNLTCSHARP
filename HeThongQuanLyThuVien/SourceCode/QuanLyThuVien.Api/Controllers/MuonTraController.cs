using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/muontra")]
public sealed class MuonTraController(IMuonTraService service) : ControllerBase
{
    [Authorize(Roles = "DOC_GIA")]
    [HttpGet("me")]
    public async Task<IActionResult> MyLoans(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue("reader_id"), out var readerId) || readerId <= 0) return Forbid();
        return Ok(await service.GetForReaderAsync(readerId, ct));
    }

    [Authorize(Roles = "DOC_GIA")]
    [HttpGet("me/{id:int}")]
    public async Task<IActionResult> MyLoan(int id, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue("reader_id"), out var readerId) || readerId <= 0) return Forbid();
        var result = await service.GetByIdForReaderAsync(readerId, id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "DOC_GIA")]
    [HttpGet("me/{id:int}/tien-phat")]
    public async Task<IActionResult> MyFines(int id, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue("reader_id"), out var readerId) || readerId <= 0) return Forbid();
        var result = await service.GetTienPhatForReaderAsync(readerId, id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] string? trangThai,
        [FromQuery] string? loaiNgay, [FromQuery] DateTime? tuNgay, [FromQuery] DateTime? denNgay,
        [FromQuery] bool chiQuaHan, CancellationToken ct)
    {
        if (tuNgay > denNgay) return BadRequest(new { message = "Từ ngày không được sau đến ngày." });
        return Ok(await service.SearchAsync(search, trangThai, loaiNgay, tuNgay, denNgay, chiQuaHan, ct));
    }

    [HttpGet("docgia/{readerId:int}")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> ReaderHistory(int readerId, CancellationToken ct) =>
        Ok(await service.GetForReaderAsync(readerId, ct));

    [HttpGet("sach-con-hang")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> Books(CancellationToken ct) => Ok(await service.GetSachConHangAsync(ct));

    [HttpGet("qua-han")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> Overdue([FromQuery] string? search, CancellationToken ct) => Ok(await service.GetQuaHanAsync(search, ct));

    [HttpGet("cau-hinh/tien-phat")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> GetRate(CancellationToken ct) => Ok(await service.GetCauHinhTienPhatAsync(ct));

    [Authorize(Roles = "ADMIN")]
    [HttpPut("cau-hinh/tien-phat")]
    public async Task<IActionResult> SetRate([FromBody] CapNhatCauHinhTienPhatDto request, CancellationToken ct)
    {
        if (request.MucPhatMoiNgay <= 0) return BadRequest(new { message = "Mức phạt mỗi ngày phải lớn hơn 0." });
        return Ok(await service.SetCauHinhTienPhatAsync(request.MucPhatMoiNgay, ct));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> Create([FromBody] TaoMuonTraDto request, CancellationToken ct)
    {
        try { var result = await service.CreateAsync(request, ct); return CreatedAtAction(nameof(Get), new { id = result.MaPhieu }, result); }
        catch (BorrowingRuleException ex) { return BadRequest(new { message = ex.Message }); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { message = "Dữ liệu đã thay đổi hoặc vi phạm ràng buộc cơ sở dữ liệu." }); }
    }

    [HttpPatch("{id:int}/chi-tiet/{detailId:int}/tra")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> Return(int id, int detailId, [FromBody] TraChiTietDto request, CancellationToken ct)
    {
        try
        {
            var result = await service.TraChiTietAsync(id, detailId, request, ct);
            return result is null ? NotFound() : Ok(result);
        }
        catch (BorrowingRuleException ex) { return BadRequest(new { message = ex.Message }); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { message = "Dữ liệu đã thay đổi hoặc vi phạm ràng buộc cơ sở dữ liệu." }); }
    }

    [HttpGet("{id:int}/tien-phat")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> Fines(int id, CancellationToken ct)
    {
        var result = await service.GetTienPhatAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:int}/tien-phat")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> UpdateFine(int id, [FromBody] CapNhatTienPhatDto request, CancellationToken ct)
    {
        try { var result = await service.UpdateTienPhatAsync(id, request, ct); return result is null ? NotFound() : Ok(result); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/tien-phat/thu")]
    [Authorize(Roles = "ADMIN,THU_THU")]
    public async Task<IActionResult> PayFine(int id, [FromBody] ThuTienPhatDto request, CancellationToken ct)
    {
        try { return Ok(await service.ThuTienPhatAsync(id, request, ct)); }
        catch (BorrowingRuleException ex) { return BadRequest(new { message = ex.Message }); }
        catch (BorrowingConflictException ex) { return Conflict(new { message = ex.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { message = "Giao dịch thu tiền vi phạm ràng buộc dữ liệu." }); }
    }
}
