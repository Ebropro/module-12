
using System.ComponentModel.DataAnnotations;

//Edit course entity
namespace TmsApi.Entities;
public class Course
{
public int Id { get; set; }
public required string Code { get; set; }
public required string Title { get; set; }
// Renamed from Capacity → MaxCapacity
public int MaxCapacity { get; set; }
public ICollection<Enrollment> Enrollments { get; set; } = [];
}





