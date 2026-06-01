using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Data;
using UniversityTasksDbFirstApi.DTOs;
using UniversityTasksDbFirstApi.Models;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/submissions")]
public class SubmissionsController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;

    public SubmissionsController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<SubmissionDto>> CreateSubmission(CreateSubmissionDto request)
    {
        // Validate student exists and is active
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == request.StudentId);
        
        if (student == null)
        {
            return BadRequest($"Student with ID {request.StudentId} does not exist.");
        }
        
        if (!student.IsActive)
        {
            return BadRequest($"Student with ID {request.StudentId} is not active.");
        }

        // Validate assignment exists and is published
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.AssignmentId == request.AssignmentId);
        
        if (assignment == null)
        {
            return BadRequest($"Assignment with ID {request.AssignmentId} does not exist.");
        }
        
        if (!assignment.IsPublished)
        {
            return BadRequest($"Assignment with ID {request.AssignmentId} is not published.");
        }

        // Validate student is enrolled in the course
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == request.StudentId 
                && e.CourseId == assignment.CourseId
                && (e.Status == "Active" || e.Status == "Completed"));
        
        if (enrollment == null)
        {
            return BadRequest($"Student is not enrolled in the course for this assignment.");
        }

        // Check for duplicate submission
        var existingSubmission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == request.AssignmentId 
                && s.StudentId == request.StudentId);
        
        if (existingSubmission != null)
        {
            return Conflict($"Student has already submitted this assignment.");
        }

        // Validate URL format
        if (string.IsNullOrWhiteSpace(request.RepositoryUrl) || !request.RepositoryUrl.StartsWith("https://"))
        {
            return BadRequest("Repository URL must start with https:// and cannot be empty.");
        }

        // Create submission
        var now = DateTime.UtcNow;
        var submission = new Submission
        {
            AssignmentId = request.AssignmentId,
            StudentId = request.StudentId,
            RepositoryUrl = request.RepositoryUrl,
            SubmittedAt = now,
            Status = assignment.IsOverdue(now) ? "Late" : "Submitted",
            Score = null,
            Feedback = null
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        // Load navigation properties for response
        await _context.Entry(submission)
            .Reference(s => s.Assignment)
            .LoadAsync();
        await _context.Entry(submission)
            .Reference(s => s.Student)
            .LoadAsync();

        var response = new SubmissionDto
        {
            SubmissionId = submission.SubmissionId,
            AssignmentId = submission.AssignmentId,
            AssignmentTitle = submission.Assignment?.Title ?? string.Empty,
            StudentId = submission.StudentId,
            StudentIndex = submission.Student?.IndexNumber ?? string.Empty,
            StudentName = submission.Student?.FullName ?? string.Empty,
            RepositoryUrl = submission.RepositoryUrl,
            SubmittedAt = submission.SubmittedAt,
            Score = submission.Score,
            Feedback = submission.Feedback,
            Status = submission.Status
        };

        return CreatedAtAction(nameof(CreateSubmission), new { id = submission.SubmissionId }, response);
    }

    [HttpPut("{idSubmission:int}/grade")]
    public async Task<IActionResult> GradeSubmission(int idSubmission, GradeSubmissionDto request)
    {
        // Load submission with assignment (demonstrating Change Tracker behavior)
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.SubmissionId == idSubmission);
        
        if (submission == null)
        {
            return NotFound($"Submission with ID {idSubmission} does not exist.");
        }

        // Validate score range
        if (request.Score < 0)
        {
            return BadRequest("Score cannot be negative.");
        }
        
        if (request.Score > submission.Assignment!.MaxPoints)
        {
            return BadRequest($"Score cannot exceed assignment max points ({submission.Assignment.MaxPoints}).");
        }

        // Update submission using Change Tracker
        submission.Score = request.Score;
        submission.Feedback = request.Feedback;
        submission.Status = "Graded";
        
        // SaveChangesAsync will detect changes automatically
        await _context.SaveChangesAsync();

        return Ok(new { message = "Submission graded successfully." });
    }

    [HttpDelete("{idSubmission:int}")]
    public async Task<IActionResult> DeleteSubmission(int idSubmission)
    {
        var submission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.SubmissionId == idSubmission);
        
        if (submission == null)
        {
            return NotFound($"Submission with ID {idSubmission} does not exist.");
        }

        // Prevent deletion of graded submissions
        if (submission.Status == "Graded")
        {
            return BadRequest("Cannot delete a submission that has already been graded.");
        }

        _context.Submissions.Remove(submission);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}