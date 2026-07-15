namespace TmsApi.Dtos;

// What the client sees — no navigation properties, no internal EF fields
// StudentId and CourseId are included so the client can follow links
// to /api/students/{studentId} or /api/courses/{courseId}
public record CertificateResponseDto(
    int Id,
    string SerialNumber,
    DateTime IssuedAt,
    int StudentId,
    int CourseId);
