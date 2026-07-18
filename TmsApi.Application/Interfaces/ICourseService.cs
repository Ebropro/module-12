using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICourseService
{
    // Exercise 1 + 2: get by int id, return DTO
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    // Exercise 2: accept request DTO, return response DTO
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);

    // Exercise 3: pre-check before insert — prevents 500 from unique index violation
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest
request, CancellationToken ct);
}