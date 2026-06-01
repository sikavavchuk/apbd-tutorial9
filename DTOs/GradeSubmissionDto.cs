using System.ComponentModel.DataAnnotations;

namespace UniversityTasksDbFirstApi.DTOs;

public class GradeSubmissionDto
{
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Score cannot be negative")]
    public int Score { get; set; }
    
    public string? Feedback { get; set; }
}