using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers;

// M9 Session 1: the instructor dashboard's SignalStore talks to this controller.
// Deliberately NOT under api/v{version}/... or api/courses/{courseId}/... —
// this is a flat, unscoped, admin-facing surface, matching the lab's assumed
// GET /api/enrollments and POST /api/enrollments/{id}/approve exactly.
[ApiController]
[Route("api/enrollments")]
public class EnrollmentAdminController(IEnrollmentService enrollmentService) : ControllerBase
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

        return Ok(updated);
    }
}
