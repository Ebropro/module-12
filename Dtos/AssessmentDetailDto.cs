namespace TmsApi.Dtos;
using Tms.Api.Dtos;
// Detail shape — returned by GET /api/courses/{courseId}/assessments/{id}
// Includes HATEOAS links: self, update, delete, parent course
public record AssessmentDetailDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required decimal MaxScore { get; init; }
    public required decimal Weight { get; init; }
    public required int CourseId { get; init; }

    public required IReadOnlyList<LinkDto> Links { get; init; }
}
