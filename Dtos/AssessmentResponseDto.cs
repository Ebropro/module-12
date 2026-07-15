namespace TmsApi.Dtos;

// What the client sees — no Course navigation property, no EF internals
// CourseId included so client can link back to /api/courses/{courseId}
public record AssessmentResponseDto(
    int Id,
    string Title,
    decimal MaxScore,
    decimal Weight,
    int CourseId);
