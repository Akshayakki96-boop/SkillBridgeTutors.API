using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/teachers")]
    [Authorize]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IRegularClassRepository _regularClassRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(
            ITeacherRepository teacherRepository,
            IRegularClassRepository regularClassRepository,
            ApplicationDbContext context,
            ILogger<TeachersController> logger)
        {
            _teacherRepository = teacherRepository;
            _regularClassRepository = regularClassRepository;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get teacher dashboard data including schedule for both demo and regular classes, students, and earnings.
        /// Teachers see both their demo classes (from leads) and regular classes (from students).
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            // Extract teacher ID from JWT
            var teacherIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!long.TryParse(teacherIdClaim, out var teacherId))
                return Unauthorized(new { message = "Invalid teacher token." });

            // Get teacher details
            var teacher = await _teacherRepository.GetByIdAsync(teacherId);
            if (teacher == null)
                return NotFound(new { message = "Teacher not found." });

            var today = DateTime.UtcNow.Date;

            // Get all demo bookings for this teacher
            var allDemoBookings = await _context.DemoBookings
                .Include(b => b.Lead)
                .Include(b => b.DemoSlot)
                .Where(b => b.TeacherId == teacherId && b.Status == "Booked")
                .ToListAsync();

            // Get all regular classes for this teacher
            var allRegularClasses = await _regularClassRepository.GetByTeacherAsync(teacherId);

            // Combine demo and regular classes for today
            var todaysClasses = new List<ScheduledClassDto>();

            // Add demo classes for today
            var todaysDemoClasses = allDemoBookings
                .Where(b => b.DemoSlot.StartTime.Date == today)
                .OrderBy(b => b.DemoSlot.StartTime)
                .Select(b => new ScheduledClassDto
                {
                    BookingId = b.BookingId,
                    StudentName = b.Lead.StudentName,
                    StudentEmail = b.Lead.Email,
                    StudentPhone = b.Lead.Phone,
                    Subject = b.Lead.Subject,
                    ClassType = "Demo",
                    StartTime = b.DemoSlot.StartTime,
                    EndTime = b.DemoSlot.EndTime,
                    MeetingLink = b.MeetingLink,
                    Status = b.Status
                })
                .ToList();

            // Add regular classes for today
            var todaysRegularClasses = allRegularClasses
                .Where(c => c.StartTime.Date == today && c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .Select(c => new ScheduledClassDto
                {
                    ClassId = c.ClassId,
                    StudentName = c.Student?.FullName ?? "Unknown",
                    StudentEmail = c.Student?.Email ?? "",
                    StudentPhone = c.Student?.Phone ?? "",
                    Subject = c.Subject,
                    ClassType = "Regular",
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    MeetingLink = c.MeetingLink,
                    Status = c.Status
                })
                .ToList();

            todaysClasses.AddRange(todaysDemoClasses);
            todaysClasses.AddRange(todaysRegularClasses);
            todaysClasses = todaysClasses.OrderBy(c => c.StartTime).ToList();

            // Upcoming classes (future, excluding today) - combine both types
            var upcomingClasses = new List<UpcomingClassDto>();

            // Add upcoming demo classes
            var upcomingDemoClasses = allDemoBookings
                .Where(b => b.DemoSlot.StartTime.Date > today)
                .OrderBy(b => b.DemoSlot.StartTime)
                .Take(20)
                .Select(b => new UpcomingClassDto
                {
                    BookingId = b.BookingId,
                    StudentName = b.Lead.StudentName,
                    Subject = b.Lead.Subject,
                    ClassType = "Demo",
                    StartTime = b.DemoSlot.StartTime,
                    EndTime = b.DemoSlot.EndTime,
                    Status = b.Status
                })
                .ToList();

            // Add upcoming regular classes
            var upcomingRegularClasses = allRegularClasses
                .Where(c => c.StartTime.Date > today && c.Status != "Cancelled")
                .OrderBy(c => c.StartTime)
                .Take(20)
                .Select(c => new UpcomingClassDto
                {
                    ClassId = c.ClassId,
                    StudentName = c.Student?.FullName ?? "Unknown",
                    Subject = c.Subject,
                    ClassType = "Regular",
                    StartTime = c.StartTime,
                    EndTime = c.EndTime,
                    Status = c.Status
                })
                .ToList();

            upcomingClasses.AddRange(upcomingDemoClasses);
            upcomingClasses.AddRange(upcomingRegularClasses);
            upcomingClasses = upcomingClasses.OrderBy(c => c.StartTime).Take(20).ToList();

            // All unique students - from both demo bookings and regular classes
            var uniqueStudents = new List<StudentDto>();

            // From demo bookings
            var demoStudents = allDemoBookings
                .GroupBy(b => b.LeadId)
                .Select(g => new StudentDto
                {
                    LeadId = g.Key,
                    StudentName = g.First().Lead.StudentName,
                    Email = g.First().Lead.Email,
                    Phone = g.First().Lead.Phone,
                    Subject = g.First().Lead.Subject,
                    ClassYear = g.First().Lead.ClassYear,
                    TotalClasses = g.Count(),
                    FirstBookedAt = g.Min(b => b.BookedAt)
                })
                .ToList();

            // From regular classes
            var regularClassStudents = allRegularClasses
                .GroupBy(c => c.StudentId)
                .Select(g => new StudentDto
                {
                    StudentId = g.Key,
                    StudentName = g.First().Student?.FullName ?? "Unknown",
                    Email = g.First().Student?.Email ?? "",
                    Phone = g.First().Student?.Phone ?? "",
                    Subject = g.First().Subject,
                    ClassYear = g.First().Student?.ClassYear,
                    TotalClasses = g.Count(),
                    FirstBookedAt = g.Min(c => c.CreatedAt)
                })
                .ToList();

            uniqueStudents.AddRange(demoStudents);
            uniqueStudents.AddRange(regularClassStudents);
            uniqueStudents = uniqueStudents
                .GroupBy(s => s.StudentName)
                .Select(g => g.First())
                .OrderBy(s => s.StudentName)
                .ToList();

            var dashboard = new TeacherDashboardDto
            {
                TeacherId = teacher.TeacherId,
                TeacherName = teacher.FullName,
                Email = teacher.Email,
                Phone = teacher.Phone,
                Subjects = teacher.Subjects,
                ClassesToday = todaysClasses.Count,
                TotalStudents = uniqueStudents.Count,
                EarningsThisMonth = 0, // TODO: Calculate from payments table if integrated
                AverageRating = 4.9m, // TODO: Implement rating system
                TodaysSchedule = todaysClasses,
                UpcomingClasses = upcomingClasses,
                AllStudents = uniqueStudents
            };

            return Ok(dashboard);
        }

        /// <summary>
        /// Get all teachers.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teachers = await _teacherRepository.GetAllAsync();
            var result = teachers.Select(t => new TeacherResponseDto
            {
                TeacherId = t.TeacherId,
                FullName  = t.FullName,
                Email     = t.Email,
                Phone     = t.Phone,
                Subjects  = t.Subjects,
                IsActive  = t.IsActive,
                CreatedAt = t.CreatedAt
            });
            return Ok(result);
        }

        /// <summary>
        /// Get a teacher by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return NotFound(new { message = "Teacher not found." });

            return Ok(new TeacherResponseDto
            {
                TeacherId = teacher.TeacherId,
                FullName  = teacher.FullName,
                Email     = teacher.Email,
                Phone     = teacher.Phone,
                Subjects  = teacher.Subjects,
                IsActive  = teacher.IsActive,
                CreatedAt = teacher.CreatedAt
            });
        }

        /// <summary>
        /// Add a new teacher.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            var teacher = new Teacher
            {
                FullName = dto.FullName,
                Email    = dto.Email,
                Phone    = dto.Phone,
                Subjects = dto.Subjects,
                IsActive = true
            };

            // If password is provided, hash it
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            var created = await _teacherRepository.CreateAsync(teacher);
            _logger.LogInformation("Teacher created — Id: {TeacherId} Name: {Name}", created.TeacherId, created.FullName);

            return CreatedAtAction(nameof(GetById), new { id = created.TeacherId }, new TeacherResponseDto
            {
                TeacherId = created.TeacherId,
                FullName  = created.FullName,
                Email     = created.Email,
                Phone     = created.Phone,
                Subjects  = created.Subjects,
                IsActive  = created.IsActive,
                CreatedAt = created.CreatedAt
            });
        }

        /// <summary>
        /// Update a teacher's details or activate/deactivate them.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateTeacherDto dto)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return NotFound(new { message = "Teacher not found." });

            teacher.FullName = dto.FullName;
            teacher.Email    = dto.Email;
            teacher.Phone    = dto.Phone;
            teacher.Subjects = dto.Subjects;
            teacher.IsActive = dto.IsActive;

            // If password is provided, hash and update it
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                teacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _teacherRepository.UpdateAsync(teacher);
            _logger.LogInformation("Teacher updated — Id: {TeacherId}", id);

            return Ok(new TeacherResponseDto
            {
                TeacherId = teacher.TeacherId,
                FullName  = teacher.FullName,
                Email     = teacher.Email,
                Phone     = teacher.Phone,
                Subjects  = teacher.Subjects,
                IsActive  = teacher.IsActive,
                CreatedAt = teacher.CreatedAt
            });
        }

        /// <summary>
        /// Delete a teacher.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return NotFound(new { message = "Teacher not found." });

            await _teacherRepository.DeleteAsync(teacher);
            _logger.LogInformation("Teacher deleted — Id: {TeacherId}", id);

            return Ok(new { message = "Teacher deleted successfully." });
        }
    }
}
