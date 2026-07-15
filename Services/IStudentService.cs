using TmsApi.Dtos;
using Tms.Api.Dtos;

public interface IStudentService
{
    // Normal queries — HasQueryFilter hides IsDeleted students automatically

    // Paginated list (matches Course pattern from M6 Session 2)
    Task<PagedResponse<StudentResponseDto>> GetAllAsync(PagedRequest request, CancellationToken ct);

    // Detail with HATEOAS links
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);

    // Create — accepts DTO, returns DTO
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);

    // Update — accepts DTO with Version for concurrency, returns DTO
    Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct);

    // Hard delete
    Task<bool> DeleteAsync(int id, CancellationToken ct);

    // Soft delete — sets IsDeleted = true, student vanishes from normal queries
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

    // Admin: bypasses HasQueryFilter via IgnoreQueryFilters()
    Task<IReadOnlyList<StudentResponseDto>> GetAllIncludingDeletedAsync(CancellationToken ct);

    // Admin: restore soft-deleted student
    Task<bool> RestoreAsync(int id, CancellationToken ct);

    // Duplicate registration number check — prevents 500 on unique constraint
    Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct);
}
