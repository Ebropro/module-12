using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface IAssessmentService
{
    // List all assessments for a course — nested under /api/courses/{courseId}/assessments
    Task<IReadOnlyList<AssessmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);

    // Get single assessment scoped to courseId
    Task<AssessmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct);

    // Create assessment under a course
    Task<AssessmentResponseDto> CreateAsync(int courseId, CreateAssessmentRequest request, CancellationToken ct);

    // Update assessment
    Task<AssessmentResponseDto?> UpdateAsync(int courseId, int id, UpdateAssessmentRequest request, CancellationToken ct);

    // Delete assessment
    Task<bool> DeleteAsync(int courseId, int id, CancellationToken ct);
}