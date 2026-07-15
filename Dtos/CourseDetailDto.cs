namespace Tms.Api.Dtos;
// Includes everything in CourseResponseDto PLUS the links array
public record CourseDetailDto
{
    public required int Id { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required int MaxCapacity { get; init; }
    public required int EnrollmentCount { get; init; }

// 5 links when capacity available, 4 when full (enroll disappears)
public required IReadOnlyList<LinkDto> Links { get; init; }
}
