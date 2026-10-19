using Library.API.Authorization;
using Library.API.Extensions;
using Library.Application.Common;
using Library.Application.DTOs.Loans;
using Library.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers;

/// <summary>השאלות: המשאב המוגבל. הקוראת תמיד נלקחת מה-token, אף פעם לא מהבקשה.</summary>
[Route("api/loans")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class LoansController : ApiControllerBase
{
    private readonly ILoanService _loans;

    public LoansController(ILoanService loans)
    {
        _loans = loans;
    }

    /// <summary>ספרנית או מנהלת: רואה ומחזירה כל השאלה.</summary>
    private bool IsStaff => User.IsInRole(Roles.Librarian) || User.IsInRole(Roles.Admin);

    /// <summary>השאלת עותק. 409 אם העותק לא פנוי, אם הגעת למכסה, או אם מישהי הקדימה אותך.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Reader)]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Borrow([FromBody] CreateLoanRequest request, CancellationToken ct) =>
        ToCreatedResult(await _loans.BorrowAsync(request.CopyId, User.GetUserId(), ct),
            nameof(GetById), loan => new { id = loan.Id });

    /// <summary>החזרה: עדכון מצב ההשאלה. בלי גוף, השרת קובע את שעת ההחזרה.</summary>
    [HttpPatch("{id:int}")]
    [Authorize(Roles = Roles.Reader + "," + Roles.Librarian)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Return([FromRoute] int id, CancellationToken ct) =>
        ToActionResult(await _loans.ReturnAsync(id, User.GetUserId(), IsStaff, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct) =>
        ToActionResult(await _loans.GetByIdAsync(id, User.GetUserId(), IsStaff, ct));

    [HttpGet("me")]
    [Authorize(Roles = Roles.Reader)]
    [ProducesResponseType(typeof(PagedResult<LoanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine([FromQuery] MyLoansQuery query, CancellationToken ct) =>
        Ok(await _loans.GetMyLoansAsync(User.GetUserId(), query, ct));

    [HttpGet]
    [Authorize(Roles = Roles.LibrarianOrAdmin)]
    [ProducesResponseType(typeof(PagedResult<LoanResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] LoansQuery query, CancellationToken ct) =>
        Ok(await _loans.GetAllAsync(query, ct));
}
