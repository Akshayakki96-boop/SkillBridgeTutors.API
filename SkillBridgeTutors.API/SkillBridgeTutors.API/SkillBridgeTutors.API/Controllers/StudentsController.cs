using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/students")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IRegularClassRepository _regularClassRepository;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            IStudentRepository studentRepository,
            IRegularClassRepository regularClassRepository,
            ILogger<StudentsController> logger)
        {
            _studentRepository = studentRepository;
            _regularClassRepository = regularClassRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get student dashboard data including their schedule and class information.
        /// Only shows the logged-in student's own regular classes.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            // Extract student ID from JWT
            var studentIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(studentIdClaim, out var studentId))
                return Unauthorized(new { message = "Invalid student token." });

            // Get student details
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return NotFound(new { message = "Student not found." });

            // Get all regular classes for this student
            var allClasses = await _regularClassRepository.GetByStudentAsync(studentId);

            var today = DateTime.UtcNow.Date;

            // Today's classes
            var todaysClasses = allClasses
                .Where(c => c.StartTime.Date == today && c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .Select(c => new StudentClassDto
                {
                    ClassId = c.ClassId,
                    TeacherName = c.Teacher?.FullName ?? "Unknown",
                    Subject = c.Subject,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    MeetingLink = c.MeetingLink,
                    Status = c.Status,
                    RecurrencePattern = c.RecurrencePattern
                })
                .ToList();

            // Upcoming classes (future, excluding today)
            var upcomingClasses = allClasses
                .Where(c => c.StartTime.Date > today && c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .Take(20) // Limit to next 20 classes
                .Select(c => new StudentClassDto
                {
                    ClassId = c.ClassId,
                    TeacherName = c.Teacher?.FullName ?? "Unknown",
                    Subject = c.Subject,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    MeetingLink = c.MeetingLink,
                    Status = c.Status,
                    RecurrencePattern = c.RecurrencePattern
                })
                .ToList();

            // Past classes (completed)
            var pastClasses = allClasses
                .Where(c => c.StartTime.Date < today || c.Status == "Completed")
                .OrderByDescending(c => c.StartTime)
                .Take(20) // Limit to last 20 classes
                .Select(c => new StudentClassDto
                {
                    ClassId = c.ClassId,
                    TeacherName = c.Teacher?.FullName ?? "Unknown",
                    Subject = c.Subject,
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    MeetingLink = c.MeetingLink,
                    Status = c.Status,
                    RecurrencePattern = c.RecurrencePattern
                })
                .ToList();

            var dashboard = new StudentDashboardDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                Email = student.Email,
                Phone = student.Phone,
                ClassYear = student.ClassYear,
                Subject = student.Subject,
                TotalClasses = allClasses.Count(),
                CompletedClasses = allClasses.Count(c => c.Status == "Completed"),
                UpcomingClasses = upcomingClasses.Count,
                TodaysClasses = todaysClasses,
                UpcomingClassesList = upcomingClasses,
                PastClasses = pastClasses
            };

            return Ok(dashboard);
        }

        /// <summary>
        /// Get a specific class details by ID (only if student is enrolled in it).
        /// </summary>
        [HttpGet("classes/{classId}")]
        public async Task<IActionResult> GetClassDetails(long classId)
        {
            // Extract student ID from JWT
            var studentIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(studentIdClaim, out var studentId))
                return Unauthorized(new { message = "Invalid student token." });

            var regularClass = await _regularClassRepository.GetByIdAsync(classId);
            if (regularClass == null || regularClass.StudentId != studentId)
                return NotFound(new { message = "Class not found." });

            var classDto = new StudentClassDto
            {
                ClassId = regularClass.ClassId,
                TeacherName = regularClass.Teacher?.FullName ?? "Unknown",
                Subject = regularClass.Subject,
                StartTime = regularClass.StartTime,
                EndTime = regularClass.EndTime,
                MeetingLink = regularClass.MeetingLink,
                Status = regularClass.Status,
                RecurrencePattern = regularClass.RecurrencePattern
            };

            return Ok(classDto);
        }

        /// <summary>
        /// Get student profile information.
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Extract student ID from JWT
            var studentIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(studentIdClaim, out var studentId))
                return Unauthorized(new { message = "Invalid student token." });

            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return NotFound(new { message = "Student not found." });

            return Ok(new
            {
                student.Id,
                student.FullName,
                student.Email,
                student.Phone,
                student.ClassYear,
                student.Subject,
                student.Address,
                student.IsActive,
                student.CreatedAt
            });
        }
    }
}
