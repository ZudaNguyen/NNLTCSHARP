using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;
using QuanLyThuVien.Domain.Enums;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/docgia")]
public sealed class DocGiaController(IDocGiaService service) : ControllerBase
{
    [Authorize(Roles = AccountRoles.Reader)]
    [HttpGet("me")]
    public async Task<ActionResult<DocGiaDto>> GetMe(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue("reader_id"), out var readerId) || readerId <= 0)
            return Forbid();
        var reader = await service.GetByIdAsync(readerId, cancellationToken);
        return reader is null ? NotFound() : Ok(reader);
    }

    [HttpGet]
    [Authorize(Roles = AccountRoles.Staff)]
    public async Task<ActionResult<IReadOnlyList<DocGiaDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? trangThai,
        CancellationToken cancellationToken) =>
        Ok(await service.SearchAsync(search, trangThai, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = AccountRoles.Staff)]
    public async Task<ActionResult<DocGiaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(new ProblemDetails { Title = "Mã độc giả phải lớn hơn 0.", Status = StatusCodes.Status400BadRequest });
        var reader = await service.GetByIdAsync(id, cancellationToken);
        return reader is null ? NotFound() : Ok(reader);
    }

    [HttpPost]
    [Authorize(Roles = AccountRoles.Staff)]
    public async Task<ActionResult<DocGiaDto>> Create(CreateDocGiaDto request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.MaDocGia }, created);
        }
        catch (DuplicateDocGiaEmailException exception) { return EmailConflict(exception); }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AccountRoles.Staff)]
    public async Task<ActionResult<DocGiaDto>> Update(int id, UpdateDocGiaDto request, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(new ProblemDetails { Title = "Mã độc giả phải lớn hơn 0.", Status = StatusCodes.Status400BadRequest });
        try
        {
            var updated = await service.UpdateAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DuplicateDocGiaEmailException exception) { return EmailConflict(exception); }
    }

    [HttpPatch("{id:int}/trang-thai")]
    [Authorize(Roles = AccountRoles.Staff)]
    public async Task<ActionResult<DocGiaDto>> UpdateTrangThai(
        int id, UpdateDocGiaTrangThaiDto request, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(new ProblemDetails { Title = "Mã độc giả phải lớn hơn 0.", Status = StatusCodes.Status400BadRequest });
        var updated = await service.UpdateTrangThaiAsync(id, request.TrangThai, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    private ConflictObjectResult EmailConflict(Exception exception) =>
        Conflict(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status409Conflict });
}
