using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/admin/scheduling")]
    [Authorize(Roles = "Admin")]
    public class AdminSchedulingController : ControllerBase
    {
        private readonly IClassSchedulingService _schedulingService;
        private readonly IStudentRepository _studentRepository;
        private readonly IRegularClassRepository _regularClassRepository;
        private readonly ITeacherAvailabilityRepository _availabilityRepository;

        public AdminSchedulingController(IClassSchedulingService schedulingService, IStudentRepository studentRepository, IRegularClassRepository regularClassRepository, ITeacherAvailabilityRepository availabilityRepository)
        {
            _schedulingService = schedulingService;
            _studentRepository = studentRepository;
            _regularClassRepository = regularClassRepository;
            _availabilityRepository = availabilityRepository;
        }

        /// <summary>
        /// Debug endpoint: Get availability for a specific teacher
        /// </summary>
        [HttpGet("teachers/{teacherId}/availability")]
        public async Task<IActionResult> GetTeacherAvailability(long teacherId)
        {
            var slots = await _availabilityRepository.GetByTeacherAsync(teacherId);
            var result = slots.Select(a => new
            {
                a.AvailabilityId,
                DayOfWeek = a.DayOfWeek.ToString(),
                a.StartTime,
                a.EndTime,
                a.IsActive
            });
            return Ok(result);
        }

        /// <summary>
        /// Lists all scheduled regular classes (excluding cancelled ones) for the admin manage classes page.
        /// </summary>
        [HttpGet("regular-classes")]
        public async Task<IActionResult> GetAllRegularClasses()
        {
            var classes = await _regularClassRepository.GetAllAsync();
            var result = classes.Select(c => new
            {
                c.ClassId,
                c.StudentId,
                StudentName = c.Student?.FullName ?? "Unknown",
                StudentEmail = c.Student?.Email ?? string.Empty,
                c.TeacherId,
                TeacherName = c.Teacher?.FullName ?? "Unknown",
                c.Subject,
                c.StartTime,
                c.EndTime,
                c.RecurrencePattern,
                c.Status,
                c.MeetingLink
            });
            return Ok(result);
        }

        /// <summary>
        /// Lists all registered students for the class assignment dropdown.
        /// </summary>
        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var students = await _studentRepository.GetAllAsync();
            var result = students.Select(s => new StudentListItemDto
            {
                Id = s.Id,
                FullName = s.FullName,
                Email = s.Email,
                Phone = s.Phone,
                ClassYear = s.ClassYear,
                Subject = s.Subject,
                IsActive = s.IsActive
            });
            return Ok(result);
        }

        /// <summary>
        /// Suggests teachers whose weekly availability covers the requested slot and who have no clashing class.
        /// </summary>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] ScheduleSuggestionRequestDto request)
        {
            var suggestions = await _schedulingService.GetSuggestionsAsync(request);
            return Ok(suggestions);
        }

        /// <summary>
        /// Admin assigns a teacher to a student as a weekly regular class. A meeting link is generated if none is supplied.
        /// </summary>
        [HttpPost("regular-classes")]
        public async Task<IActionResult> CreateRegularClass([FromBody] CreateRegularClassDto dto)
        {
            var result = await _schedulingService.CreateRegularClassAsync(dto);
            if (result.Class == null)
                return BadRequest(new { message = result.Error });

            var created = result.Class;
            return Ok(new
            {
                created.ClassId,
                created.StudentId,
                created.TeacherId,
                created.Subject,
                created.StartTime,
                created.EndTime,
                created.RecurrencePattern,
                created.Status,
                created.MeetingLink
            });
        }

        /// <summary>
        /// Reschedules an existing regular class to a new slot, re-validating clashes and emailing the student and teacher.
        /// </summary>
        [HttpPut("regular-classes/{classId}/reschedule")]
        public async Task<IActionResult> RescheduleRegularClass(long classId, [FromBody] RescheduleRegularClassDto dto)
        {
            var result = await _schedulingService.RescheduleRegularClassAsync(classId, dto);
            if (result.Class == null)
                return BadRequest(new { message = result.Error });

            var updated = result.Class;
            return Ok(new
            {
                updated.ClassId,
                updated.StudentId,
                updated.TeacherId,
                updated.Subject,
                updated.StartTime,
                updated.EndTime,
                updated.RecurrencePattern,
                updated.Status,
                updated.MeetingLink
            });
        }
    }
}
