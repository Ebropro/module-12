using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
// M7 Session 2 — Exercise 3, Step 5: this handler used to query TmsDbContext
// directly. It now goes through ICachedCourseService so that repeated reads
// of the same page are served from HybridCache instead of hitting the DB
// every time — this is the endpoint the Exercise 3 load tests hit.
public class CoursesController(
    ICourseService courseService,
    ICachedCourseService cachedCourseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var request = new PagedRequest { Page = page, PageSize = pageSize };
        var result = await cachedCourseService.GetCoursesAsync(request, ct);

        var rows = result.Items.Select(c => new
        {
            c.Id,
            c.Title,
            c.Code,
            c.MaxCapacity,
            c.EnrollmentCount
        });

        return Ok(new
        {
            data = rows,
            meta = new
            {
                totalCount = result.TotalCount,
                page = result.Page,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                hasNext = result.HasNext,
                hasPrevious = result.HasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={page}&pageSize={pageSize}",
                next = result.HasNext
                    ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                    : (string?)null,
                prev = result.HasPrevious
                    ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                    : (string?)null,
                enroll = "/api/v2/enrollments"
            }
        });
    }

    // M7 Session 2 — Exercise 3, Step 6: write path. Updates via ICourseService
    // (uncached — always hits the DB), then invalidates the "courses" tag so the
    // next GET produces a fresh MISS instead of serving the pre-update cached page.
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(
        int id, UpdateCourseRequest request, CancellationToken ct)
    {
        var updated = await courseService.UpdateAsync(id, request, ct);
        if (updated is null)
            return NotFound();

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return Ok(updated);
    }

    //

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCourse(
    int id,
    CancellationToken ct)
    {
        //Temporary delay
        await Task.Delay(3000, ct);
        var result = await courseService.DeleteAsync(id, ct);

        if (!result.Found)
            return NotFound();

        if (result.HasEnrollments)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Course deletion rejected",
                Detail =
                    $"Course {result.CourseCode} cannot be deleted because it has active student enrollments."
            });
        }

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return NoContent();
    }

}