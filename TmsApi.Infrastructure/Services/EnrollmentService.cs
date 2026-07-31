using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    // ExistsAsync // AnyAsync → SELECT EXISTS (SELECT 1 LIMIT 1) — fastest existence check
    public Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .AnyAsync(e =>
                e.StudentId == studentId &&
                e.Course.Code == courseCode, // EF translates this to a JOIN
                ct);

    // AddAsync //  M7 Session 2 Step 6 (cache invalidation on writes):
    
    public async Task AddAsync(Enrollment enrollment, CancellationToken ct)
    {
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);
    }

    // GetByStudentIdAsync 
    // Called by GetStudentScheduleHandler: list all courses a student is in

    public Task<List<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.StudentId == studentId)
            .Include(e => e.Course) // required — handler reads e.Course.Code and e.Course.Title
            .ToListAsync(ct);

    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e =>
                e.Id == id &&
                e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    // Session 3 Exercise 5: List all enrollments for a course

    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId, CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .ToListAsync(ct);

    // ── GetAllSummaryAsync ────────────────────────────────────
    // M9 Session 1: powers the instructor dashboard's EnrollmentStore.loadEnrollments().
    // Unscoped by course/student on purpose — this is the "everything, right now"
    // view an instructor triages during Enrollment Week.
    public async Task<IReadOnlyList<EnrollmentSummaryDto>> GetAllSummaryAsync(CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentSummaryDto(
                e.Id,
                e.StudentId,
                e.Student.Name,
                e.CourseId,
                e.Course.Title,
                e.Status.ToString(),
                e.EnrolledAt))
            .ToListAsync(ct);

    // ── ApproveAsync ──────────────────────────────────────────
    // M9 Session 1: the server-side half of the store's optimistic approve flow.
    // The Angular store already flipped the status locally before this call
    // lands — if this fails, the store rolls back to "Pending" client-side.
    public async Task<EnrollmentSummaryDto?> ApproveAsync(int id, CancellationToken ct)
    {
        var enrollment = await context.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (enrollment is null)
            return null;

        enrollment.Status = EnrollmentStatus.Approved;
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Approved enrollment {EnrollmentId}", enrollment.Id);

        return new EnrollmentSummaryDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.Student.Name,
            enrollment.CourseId,
            enrollment.Course.Title,
            enrollment.Status.ToString(),
            enrollment.EnrolledAt);
    }
        

    // Exercise 3: Write path// Capacity check lives in the CONTROLLER
    public async Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow,
            Year = DateTime.UtcNow.Year
        };

        context.Enrollments.Add(enrollment);

        await context.SaveChangesAsync(ct);

        logger.LogInformation(
            "Created enrollment {EnrollmentId} for course {CourseId}",
            enrollment.Id,
            courseId);

        return (await GetByIdAsync(
            courseId,
            enrollment.Id,
            ct))!;
    }

    // =============Previous Module 5 ==================//
    public async Task<int> ArchiveOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default)
    {
        var affected = await context.Enrollments
            .Where(e => e.EnrolledAt < cutoff && !e.IsArchived)
            .ExecuteUpdateAsync(
                s => s.SetProperty(e => e.IsArchived, true),
                cancellationToken);

        logger.LogInformation(
            "Archived {Count} enrollments older than {Cutoff:O}", affected, cutoff);

        return affected;
    }
}