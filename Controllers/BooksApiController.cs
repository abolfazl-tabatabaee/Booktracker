using bookTracker.Common;
using bookTracker.Models.DTOs;
using bookTracker.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers;

[ApiController]
[Route("api/books")]
public class BooksApiController : ControllerBase
{
    private readonly IBooksService _books;
    private readonly IAntiforgery _antiforgery;

    private const string AntiForgeryCookieName = "bookTracker.AntiCsrf";

    public BooksApiController(IBooksService books, IAntiforgery antiforgery)
    {
        _books = books;
        _antiforgery = antiforgery;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookListDto>>> GetAll(CancellationToken ct)
    {
        var data = await _books.GetAllAsync(ct);
        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDetailsDto>> GetOne(int id, CancellationToken ct)
    {
        var dto = await _books.GetOneAsync(id, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    [Authorize]
    [HttpPost("{id:int}/reviews")]
    public async Task<ActionResult> AddReview(int id, [FromBody] CreateReviewDto input, CancellationToken ct)
    {
        if (Request.Cookies.ContainsKey(AntiForgeryCookieName))
        {
            await _antiforgery.ValidateRequestAsync(HttpContext);
        }

        var res = await _books.AddReviewAsync(id, User, input, ct);

        return res.Kind switch
        {
            ResultKind.Ok => Ok(),
            ResultKind.NotFound => NotFound(),
            ResultKind.Unauthorized => Unauthorized(),
            ResultKind.Validation => BadRequest(res.Error ?? "Invalid"),
            _ => StatusCode(500, res.Error ?? "Server error")
        };
    }
}
