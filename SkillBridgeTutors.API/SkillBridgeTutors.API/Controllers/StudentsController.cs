using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Helpers;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/students")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get student dashboard information including classes and stats
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var studentIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (studentIdClaim == null || !int.TryParse(studentIdClaim.Value, out int studentId))
                {
                    return Unauthorized(new { message = "Invalid or missing student ID" });
                }

                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                {
                    return NotFound(new { message = "Student not found" });
                }

                // Get all classes for the student
                var allClasses = await _context.RegularClasses
                    .Where(c => c.StudentId == studentId)
                    .Include(c => c.Teacher)
                    .OrderBy(c => c.StartTime)
                    .ToListAsync();

                var now = DateTime.UtcNow;
                var todayStart = now.Date;
                var todayEnd = todayStart.AddDays(1);

                // Categorize classes
                var todaysClasses = allClasses
                    .Where(c => c.StartTime >= todayStart && c.StartTime < todayEnd)
                    .Select(c => new StudentClassDto
                    {
                        ClassId = c.ClassId,
                        TeacherName = c.Teacher?.FullName ?? "Unknown Teacher",
                        Subject = c.Subject ?? string.Empty,
                        StartTime = c.StartTime,
                        EndTime = c.EndTime,
                        MeetingLink = c.MeetingLink,
                        Status = GetClassStatus(c.StartTime, c.EndTime, now),
                        RecurrencePattern = c.RecurrencePattern
                    })
                    .ToList();

                var upcomingClasses = allClasses
                    .Where(c => c.StartTime >= todayEnd)
                    .Select(c => new StudentClassDto
                    {
                        ClassId = c.ClassId,
                        TeacherName = c.Teacher?.FullName ?? "Unknown Teacher",
                        Subject = c.Subject ?? string.Empty,
                        StartTime = c.StartTime,
                        EndTime = c.EndTime,
                        MeetingLink = c.MeetingLink,
                        Status = "Scheduled",
                        RecurrencePattern = c.RecurrencePattern
                    })
                    .ToList();

                var pastClasses = allClasses
                    .Where(c => c.EndTime < now)
                    .Select(c => new StudentClassDto
                    {
                        ClassId = c.ClassId,
                        TeacherName = c.Teacher?.FullName ?? "Unknown Teacher",
                        Subject = c.Subject ?? string.Empty,
                        StartTime = c.StartTime,
                        EndTime = c.EndTime,
                        MeetingLink = c.MeetingLink,
                        Status = "Completed",
                        RecurrencePattern = c.RecurrencePattern
                    })
                    .ToList();

                var dashboard = new StudentDashboardDto
                {
                    StudentId = student.Id,
                    StudentName = student.FullName ?? string.Empty,
                    Email = student.Email ?? string.Empty,
                    Phone = PlaceholderText.Clean(student.Phone) ?? string.Empty,
                    ClassYear = PlaceholderText.Clean(student.ClassYear),
                    Subject = "Maths",
                    TotalClasses = allClasses.Count,
                    CompletedClasses = pastClasses.Count,
                    UpcomingClasses = upcomingClasses.Count,
                    TodaysClasses = todaysClasses,
                    UpcomingClassesList = upcomingClasses,
                    PastClasses = pastClasses
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching dashboard", error = ex.Message });
            }
        }

        private string GetClassStatus(DateTime startTime, DateTime endTime, DateTime now)
        {
            if (now < startTime)
                return "Scheduled";
            if (now >= startTime && now < endTime)
                return "In Progress";
            return "Completed";
        }
    }
}
