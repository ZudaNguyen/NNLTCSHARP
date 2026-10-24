using Microsoft.AspNetCore.Mvc;
using QuanLyThuVien.Application.DTOs;
using QuanLyThuVien.Application.Interfaces;
using QuanLyThuVien.Application.Services;

namespace QuanLyThuVien.Api.Controllers;

[ApiController]
[Route("api/sach")]
public sealed class SachController(ISachService sachService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SachDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? maTheLoai,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] bool onlyDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var books = await sachService.SearchAsync(
            search, maTheLoai, includeDeleted, onlyDeleted, cancellationToken);
        return Ok(books);
    }

    [HttpGet("lookups")]
    public async Task<ActionResult<SachLookupsDto>> GetLookups(CancellationToken cancellationToken) =>
        Ok(await sachService.GetLookupsAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SachDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await sachService.GetByIdAsync(id, cancellationToken);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPost]
    public async Task<ActionResult<SachDto>> Create(
        [FromBody] SachWriteDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var created = await sachService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.MaSach }, created);
        }
        catch (DuplicateIsbnException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status409Conflict });
        }
        catch (BookRuleException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SachDto>> Update(
        int id,
        [FromBody] SachWriteDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await sachService.UpdateAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DuplicateIsbnException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status409Conflict });
        }
        catch (BookRuleException exception)
        {
            return BadRequest(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await sachService.SoftDeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        try
        {
            var restored = await sachService.RestoreAsync(id, cancellationToken);
            return restored ? NoContent() : NotFound();
        }
        catch (BookRuleException exception)
        {
            return Conflict(new ProblemDetails { Title = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }
}
