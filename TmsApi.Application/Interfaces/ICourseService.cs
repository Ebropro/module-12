using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    Task<Course?> GetByCodeAsync(string code, CancellationToken ct);
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    // Exercise 2: accept request DTO, return response DTO
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);

    // M7 Session 2 — Exercise 3, Step 6 support: write path the PUT endpoint uses.
    // Returns null if the course doesn't exist, so the controller can 404.
    Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseRequest request, CancellationToken ct);

    // Exercise 3: pre-check before insert — prevents 500 from unique index violation
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    Task<(bool Found, bool HasEnrollments, string? CourseCode)> DeleteAsync(
        int id,
        CancellationToken ct);




    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest
request, CancellationToken ct);
}