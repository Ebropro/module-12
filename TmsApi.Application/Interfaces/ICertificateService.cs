using TmsApi.Application.Dtos;

namespace TmsApi.Application.Interfaces;

public interface ICertificateService
{
    // Get single certificate — returns detail DTO with HATEOAS links
    Task<CertificateResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    // List all certificates for a specific student
    // GET /api/students/{studentId}/certificates
    Task<IReadOnlyList<CertificateResponseDto>> GetByStudentAsync(int studentId, CancellationToken ct);

    // List all certificates for a specific course
    // GET /api/courses/{courseId}/certificates
    Task<IReadOnlyList<CertificateResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);

    // Create certificate — validates student and course exist in service
    Task<CertificateResponseDto> CreateAsync(CreateCertificateRequest request, CancellationToken ct);

    // Delete certificate (hard delete — certificates don't soft-delete)
    Task<bool> DeleteAsync(int id, CancellationToken ct);

    // Duplicate serial number check — prevents 500 on unique constraint
    Task<bool> SerialNumberExistsAsync(string serialNumber, CancellationToken ct);
}