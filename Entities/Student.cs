
namespace TmsApi.Entities;
public class Student
{
    public int Id { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Name { get; set; }
    public int Age { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;


    // EX-8 Shadow property — LastUpdated lives in DB only
    public int Version { get; set; } // Concurrency
    
    // ✅ EX-9: Task 1: Soft-delete flag — filtered out of normal queries via HasQueryFilter
    public bool IsDeleted { get; set; } = false;

    public ICollection<Enrollment> Enrollments { get; set; } =
        new List<Enrollment>();
        
}
