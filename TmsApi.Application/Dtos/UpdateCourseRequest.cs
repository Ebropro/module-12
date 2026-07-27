using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record UpdateCourseRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    // Optional — only applied if the caller sends it, so a title-only
    
    [Range(1, 200)]
    public int? MaxCapacity { get; init; }
}
