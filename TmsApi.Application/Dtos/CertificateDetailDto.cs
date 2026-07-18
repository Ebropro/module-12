namespace TmsApi.Application.Dtos;

// Detail shape — returned by GET /api/certificates/{id}
// Includes everything in CertificateResponseDto plus HATEOAS links
public record CertificateDetailDto
{
    public required int Id { get; init; }
    public required string SerialNumber { get; init; }
    public required DateTime IssuedAt { get; init; }
    public required int StudentId { get; init; }
    public required int CourseId { get; init; }

    // HATEOAS links — self, the student, the course
    public required IReadOnlyList<LinkDto> Links { get; init; }
}
