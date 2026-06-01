using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Data;
using UniversityTasksDbFirstApi.DTOs;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentsController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;

    public StudentsController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet("{idStudent:int}/dashboard")]
    public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(int idStudent)
    {
        // Using Include and ThenInclude to avoid N+1 problem
        var student = await _context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .Include(s => s.Submissions)
                .ThenInclude(sub => sub.Assignment)
            .FirstOrDefaultAsync(s => s.StudentId == idStudent);

        if (student == null)
        {
            return NotFound($"Student with ID {idStudent} does not exist.");
        }

        var dashboard = new StudentDashboardDto
        {
            StudentId = student.StudentId,
            IndexNumber = student.IndexNumber,
            FullName = student.FullName,
            IsActive = student.IsActive,
            Enrollments = student.Enrollments.Select(e => new EnrollmentInfoDto
            {
                CourseId = e.CourseId,
                CourseCode = e.Course?.Code ?? string.Empty,
                CourseName = e.Course?.Name ?? string.Empty,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status
            }).ToList(),
            Submissions = student.Submissions.Select(sub => new SubmissionInfoDto
            {
                SubmissionId = sub.SubmissionId,
                AssignmentId = sub.AssignmentId,
                AssignmentTitle = sub.Assignment?.Title ?? string.Empty,
                RepositoryUrl = sub.RepositoryUrl,
                SubmittedAt = sub.SubmittedAt,
                Score = sub.Score,
                Feedback = sub.Feedback,
                Status = sub.Status
            }).ToList()
        };

        return Ok(dashboard);
    }
}