namespace TmsApi.Domain.Entities;

public class Student
{
    public int Id { get; set; }

    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public int Age { get; set; }

    public decimal GPA { get; set; }

    public bool IsActive { get; set; } = true;

    // Identity user associated with this student.
    public string? UserId { get; set; }

    // EX-8 Shadow property — LastUpdated lives in DB only
    public int Version { get; set; }

    public bool IsDeleted { get; set; } = false;

    public ICollection<Enrollment> Enrollments { get; set; } =
        new List<Enrollment>();
}