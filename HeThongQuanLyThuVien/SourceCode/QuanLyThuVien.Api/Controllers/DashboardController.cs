using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = AccountRoles.Staff)]
public sealed class DashboardController(IThongKeService service) : ControllerBase
{
    [HttpGet("tong-quan")]
    public async Task<ActionResult<DashboardTongQuanDto>> TongQuan(CancellationToken ct) =>
        Ok(await service.GetDashboardTongQuanAsync(ct));

    [HttpGet("hoat-dong")]
    public async Task<ActionResult<IReadOnlyList<DashboardHoatDongDto>>> HoatDong(CancellationToken ct) =>
        Ok(await service.GetDashboardHoatDongAsync(ct));
}
