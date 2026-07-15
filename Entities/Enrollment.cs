using System;
namespace TmsApi.Entities;
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; }

    public int Year { get; set; } // session 3

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // EX-9: Task 2: Bulk-archive with ExecuteUpdateAsync
    public bool IsArchived { get; set; } = false;

    public Student Student { get; set; } = null!;
   
    public Course Course { get; set; } = null!;
}
