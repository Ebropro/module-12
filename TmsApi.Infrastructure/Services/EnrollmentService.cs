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
    // Exercise 3: Read path
    // never returns an enrollment that belongs to a different course
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
            // Implement GetByCourseAsync 
            // Same pattern as GetByIdAsync — AsNoTracking, Where, Select, ToListAsync
        public async Task<IReadOnlyList<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId, CancellationToken ct) =>
        await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
            e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .ToListAsync(ct);

    // Exercise 3: Write path 
    // Capacity check lives in the CONTROLLER
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