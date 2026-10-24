using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/tacgia")]
public sealed class TacGiaController(ITacGiaService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TacGiaDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] bool onlyDeleted = false,
        CancellationToken cancellationToken = default) =>
        Ok(await service.SearchAsync(search, includeDeleted, onlyDeleted, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TacGiaDto>> GetById(
        int id, [FromQuery] bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var entity = await service.GetByIdAsync(id, includeDeleted, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    public async Task<ActionResult<TacGiaDto>> Create(
        [FromBody] TacGiaWriteDto request, CancellationToken cancellationToken)
    {
        try
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.MaTacGia }, created);
        }
        catch (DuplicateTacGiaNameException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TacGiaDto>> Update(
        int id, [FromBody] TacGiaWriteDto request, CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.UpdateAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DuplicateTacGiaNameException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            return await service.SoftDeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (TacGiaInUseException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken) =>
        await service.RestoreAsync(id, cancellationToken) ? NoContent() : NotFound();
}
