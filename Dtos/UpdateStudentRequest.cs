using System.ComponentModel.DataAnnotations;
namespace TmsApi.Dtos;

public record UpdateStudentRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Range(16, 100, ErrorMessage = "Age must be between 16 and 100.")]
    public int Age { get; init; }

    [Range(0.0, 4.0, ErrorMessage = "GPA must be between 0.0 and 4.0.")]
    public decimal GPA { get; init; }

    public bool IsActive { get; init; }

    // Version must be sent back for optimistic concurrency check
    // Client reads Version from GET, sends it back on PUT
    public int Version { get; init; }
}
