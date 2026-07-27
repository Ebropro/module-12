using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers.V2;

// M7 Session 2 — Exercise 4, Step 2b: minimal stub so the concurrency limiter
// on "transcripts" has a real HTTP surface to measure. Exercise 5 replaces the
// body with enqueue + 202 Accepted + Location header + background worker.
[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController : ControllerBase
{
//     [HttpPost]
//     [EnableRateLimiting("transcripts")]
//     public IActionResult RequestTranscript([FromBody] object? _)
//     {
//         return Ok();
//     }
// }
    [HttpPost]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript([FromBody] object? _)
    {
        await Task.Delay(10000); // simulate transcript processing

        return Ok();
    }
}