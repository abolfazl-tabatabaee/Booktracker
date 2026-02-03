using bookTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace bookTracker.Controllers;

[ApiController]
[Route("api/stats")]
public class StatsApiController : ControllerBase
{
    private readonly IStatsService _stats;
    public StatsApiController(IStatsService stats) => _stats = stats;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var (totalBooks, totalReviews, genres) = await _stats.GetAsync(ct);

        return Ok(new
        {
            totalBooks,
            totalReviews,
            genres
        });
    }
}



