using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

// M9-1: the instructor dashboard's SignalStore talks to this controller.
// Deliberately NOT under api/v{version}/... or api/courses/{courseId}/...
// GET /api/enrollments and POST /api/enrollments/{id}/approve exactly.
[ApiController]
[Route("api/enrollments")]
public class EnrollmentAdminController(IEnrollmentService enrollmentService,
IHubContext<TmsHub, ITmsHubClient> hubContext
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var enrollments = await enrollmentService.GetAllSummaryAsync(ct);
        return Ok(enrollments);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var updated = await enrollmentService.ApproveAsync(id, ct);
        if (updated is null)
            return NotFound();
        // M9- After the database commit succeeds, 
        // broadcast to all connected Angular clients
                await hubContext.Clients.All
        .ReceiveEnrollmentStatusUpdated(id.ToString(), "Approved");

        return Ok(updated);
    }
}
