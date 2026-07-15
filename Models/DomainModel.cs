
namespace TmsApi.Model;

public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);

public class Course 
{ 
    public required string Code { get; init; } 
 
    public required string Title 
    { 
        get; 
        set => field = !string.IsNullOrWhiteSpace(value) 
            ? value 
            : throw new ArgumentException("Title cannot be empty or whitespace.", nameof(value)); 
    }

}


public class Student
{
     public required string Id { get; init; }
      public required string Name
    {
        get; 
        set => field = !string.IsNullOrWhiteSpace(value) 
            ? value 
            : throw new ArgumentException("Name cannot be empty or whitespace.", nameof(value));
    }

    
}





 

