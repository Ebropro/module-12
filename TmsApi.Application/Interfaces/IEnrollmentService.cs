using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentService
{
    // Exercise 3 nested route: get enrollment by courseId + enrollmentId
    Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);
    // Exercise 3: create enrollment under a course
    Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);

    // Session 3 Exercise 5: Add GetByCourseAsync // list ALL enrollments for a course 
    // Used by GET /api/courses/{courseId}/enrollments
    Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);

    // Module 5 bulk archive (kept from previous session)
    Task<int> ArchiveOlderThanAsync(DateTime cutoff, CancellationToken cancellationToken = default);
}

