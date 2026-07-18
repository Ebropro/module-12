namespace TmsApi.Application.Dtos;

// What the client sees in the list/create/update response
// Never the raw Student entity — no IsDeleted, no Version, no Enrollments
public record StudentResponseDto(
    int Id,
    string RegistrationNumber,
    string Name,
    int Age,
    decimal GPA,
    bool IsActive,
    int EnrollmentCount);
