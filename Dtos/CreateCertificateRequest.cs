using System.ComponentModel.DataAnnotations;
namespace TmsApi.Dtos;

public record CreateCertificateRequest
{
    // SerialNumber uniquely identifies the certificate
    // Pattern: CERT-YYYY-NNNNNN e.g. CERT-2026-000001
    [Required]
    [RegularExpression(@"^CERT-\d{4}-\d{6}$",
        ErrorMessage = "SerialNumber must follow the pattern CERT-YYYY-NNNNNN (e.g. CERT-2026-000001).")]
    public required string SerialNumber { get; init; }

    // Which student receives this certificate
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    public required int StudentId { get; init; }

    // Which course the certificate is for
    [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer.")]
    public required int CourseId { get; init; }

    // Optional override — defaults to UtcNow in the service if not provided
    public DateTime? IssuedAt { get; init; }
}
