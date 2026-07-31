namespace TmsApi.Application.Dtos;

// M9 Session 1: flat, denormalized shape for the instructor dashboard's
// EnrollmentStore. Deliberately different from EnrollmentResponseDto (which
// is scoped per-course and has no names/status) — this is a separate,
// admin-facing read model, not a replacement for the existing DTO.
public record EnrollmentSummaryDto(
    int Id,
    int StudentId,
    string StudentName,
    int CourseId,
    string CourseName,
    string Status,
    DateTime EnrolledAt);
