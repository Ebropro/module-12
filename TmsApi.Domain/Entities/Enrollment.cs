namespace TmsApi.Domain.Entities;
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; }

    public int Year { get; set; } // session 3

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    
    // M9 Session 1: approval workflow status, drives the instructor dashboard's SignalStore
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

    // EX-9: Task 2: Bulk-archive with ExecuteUpdateAsync
    public bool IsArchived { get; set; } = false;

    public Student Student { get; set; } = null!;
   
    public Course Course { get; set; } = null!;
}
