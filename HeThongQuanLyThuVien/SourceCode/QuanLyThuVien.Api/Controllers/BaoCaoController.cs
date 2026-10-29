using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/baocao")]
[Authorize(Roles = "ADMIN,THU_THU")]
public sealed class BaoCaoController(IThongKeService service) : ControllerBase
{
    [HttpGet("sach-dang-muon")]
    public async Task<ActionResult<IReadOnlyList<SachDangMuonBaoCaoDto>>> SachDangMuon(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        try { return Ok(await service.GetSachDangMuonAsync(from, to, ct)); }
        catch (ReportRequestException ex) { return BadRequest(new ProblemDetails { Title = ex.Message, Status = 400 }); }
    }

    [HttpGet("sach-muon-nhieu")]
    public async Task<ActionResult<IReadOnlyList<SachMuonNhieuDto>>> SachMuonNhieu(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int top = 10, CancellationToken ct = default)
    {
        try { return Ok(await service.GetSachMuonNhieuAsync(from, to, top, ct)); }
        catch (ReportRequestException ex) { return BadRequest(new ProblemDetails { Title = ex.Message, Status = 400 }); }
    }

    [HttpGet("tien-phat")]
    public async Task<ActionResult<TienPhatBaoCaoDto>> TienPhat(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        try { return Ok(await service.GetTienPhatAsync(from, to, ct)); }
        catch (ReportRequestException ex) { return BadRequest(new ProblemDetails { Title = ex.Message, Status = 400 }); }
    }
}
