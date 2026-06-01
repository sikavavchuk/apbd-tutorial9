using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityTasksDbFirstApi.Data;
using UniversityTasksDbFirstApi.DTOs;

namespace UniversityTasksDbFirstApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly UniversityTasksDbContext _context;

    public CoursesController(UniversityTasksDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses([FromQuery] bool activeOnly = true)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Include(c => c.Assignments)
            .Where(c => !activeOnly || c.IsActive);

        var courses = await query
            .Select(c => new CourseDto
            {
                CourseId = c.CourseId,
                Code = c.Code,
                Name = c.Name,
                Credits = c.Credits,
                AssignmentCount = c.Assignments.Count
            })
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{idCourse:int}/assignments")]
    public async Task<ActionResult<IEnumerable<AssignmentDto>>> GetAssignmentsForCourse(
        int idCourse, 
        [FromQuery] bool publishedOnly = true)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == idCourse);

        if (course == null)
        {
            return NotFound($"Course with ID {idCourse} does not exist.");
        }

        var query = _context.Assignments
            .AsNoTracking()
            .Include(a => a.Submissions)
            .Where(a => a.CourseId == idCourse);

        if (publishedOnly)
        {
            query = query.Where(a => a.IsPublished);
        }

        var assignments = await query
            .Select(a => new AssignmentDto
            {
                AssignmentId = a.AssignmentId,
                Title = a.Title,
                DueDate = a.DueDate,
                MaxPoints = a.MaxPoints,
                IsPublished = a.IsPublished,
                SubmissionCount = a.Submissions.Count
            })
            .ToListAsync();

        return Ok(assignments);
    }
}