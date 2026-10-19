using Library.API.Authorization;
using Library.Application.DTOs.Copies;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

/// <summary>
/// עותקים: רשימה ויצירה תחת הספר (/api/books/{bookId}/copies),
/// ופעולות על עותק בודד לפי המזהה שלו (/api/copies/{id}).
/// </summary>
[Route("api")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class CopiesController : ApiControllerBase
{
    private readonly ICopyService _copies;

    public CopiesController(ICopyService copies)
    {
        _copies = copies;
    }

    [HttpGet("books/{bookId:int}/copies")]
    [ProducesResponseType(typeof(List<CopyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByBook([FromRoute] int bookId, CancellationToken ct) =>
        ToActionResult(await _copies.GetByBookIdAsync(bookId, ct));

    [HttpGet("copies/{id:int}")]
    [ProducesResponseType(typeof(CopyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct) =>
        ToActionResult(await _copies.GetByIdAsync(id, ct));

    [HttpPost("books/{bookId:int}/copies")]
    [Authorize(Roles = Roles.LibrarianOrAdmin)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(CopyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add([FromRoute] int bookId, [FromBody] CopyRequest request, CancellationToken ct) =>
        ToCreatedResult(await _copies.AddAsync(bookId, request, ct), nameof(GetById), copy => new { id = copy.Id });

    /// <summary>סימון עותק כ-Lost (אבד/נהרס) או החזרתו ל-Available.</summary>
    [HttpPatch("copies/{id:int}")]
    [Authorize(Roles = Roles.LibrarianOrAdmin)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus([FromRoute] int id, [FromBody] CopyStatusRequest request, CancellationToken ct) =>
        ToActionResult(await _copies.ChangeStatusAsync(id, request, ct));

    [HttpDelete("copies/{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct) =>
        ToActionResult(await _copies.DeleteAsync(id, ct));
}
