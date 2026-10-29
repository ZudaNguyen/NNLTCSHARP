using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/nhanvien")]
[Authorize(Roles = AccountRoles.Admin)]
public sealed class NhanVienController(INhanVienService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NhanVienDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? trangThai, [FromQuery] string? chucVu,
        CancellationToken ct) => Ok(await service.SearchAsync(search, trangThai, chucVu, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NhanVienDto>> GetById(int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest(new ProblemDetails { Title = "Mã nhân viên phải lớn hơn 0.", Status = StatusCodes.Status400BadRequest });
        var employee = await service.GetByIdAsync(id, ct);
        return employee is null ? NotFound() : Ok(employee);
    }

    [HttpPost]
    public async Task<ActionResult<NhanVienDto>> Create([FromBody] CreateNhanVienDto request, CancellationToken ct)
    {
        try
        {
            var created = await service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.MaNhanVien }, created);
        }
        catch (DuplicateNhanVienDataException ex) { return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict }); }
        catch (DbUpdateException) { return Conflict(new ProblemDetails { Title = "Dữ liệu nhân viên trùng hoặc vi phạm ràng buộc database.", Status = StatusCodes.Status409Conflict }); }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<NhanVienDto>> Update(int id, [FromBody] UpdateNhanVienDto request, CancellationToken ct)
    {
        if (id <= 0) return BadRequest(new ProblemDetails { Title = "Mã nhân viên phải lớn hơn 0.", Status = StatusCodes.Status400BadRequest });
        try
        {
            var updated = await service.UpdateAsync(id, request, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DuplicateNhanVienDataException ex) { return Conflict(new ProblemDetails { Title = ex.Message, Status = StatusCodes.Status409Conflict }); }
        catch (DbUpdateException) { return Conflict(new ProblemDetails { Title = "Dữ liệu nhân viên trùng hoặc vi phạm ràng buộc database.", Status = StatusCodes.Status409Conflict }); }
    }

    [HttpPatch("{id:int}/trang-thai")]
    public async Task<ActionResult<NhanVienDto>> UpdateStatus(int id, [FromBody] UpdateTrangThaiNhanVienDto request, CancellationToken ct)
    {
        if (id <= 0) return BadRequest(new ProblemDetails { Title = "Mã nhân viên phải lớn hơn 0.", Status = StatusCodes.Status400BadRequest });
        try
        {
            var updated = await service.UpdateTrangThaiAsync(id, request.TrangThai, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DbUpdateException) { return Conflict(new ProblemDetails { Title = "Trạng thái nhân viên vi phạm ràng buộc database.", Status = StatusCodes.Status409Conflict }); }
    }

    [HttpGet("{id:int}/lich-su-trang-thai")]
    public async Task<IActionResult> GetStatusHistory(int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest(new ProblemDetails { Title = "Mã nhân viên phải lớn hơn 0.", Status = StatusCodes.Status400BadRequest });
        var history = await service.GetStatusHistoryAsync(id, ct);
        return history is null ? NotFound() : Ok(history);
    }
}
