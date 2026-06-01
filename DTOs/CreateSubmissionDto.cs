using System.ComponentModel.DataAnnotations;

namespace UniversityTasksDbFirstApi.DTOs;

public class CreateSubmissionDto
{
    [Required]
    public int AssignmentId { get; set; }
    
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    [Url]
    [RegularExpression(@"^https://.*$", ErrorMessage = "Repository URL must start with https://")]
    public string RepositoryUrl { get; set; } = string.Empty;
}