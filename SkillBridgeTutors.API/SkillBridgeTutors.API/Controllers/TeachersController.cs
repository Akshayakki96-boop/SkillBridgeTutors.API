using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(ITeacherRepository teacherRepository, ILogger<TeachersController> logger)
        {
            _teacherRepository = teacherRepository;
            _logger = logger;
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
                Subjects = dto.Subjects,
                IsActive = true
            };

            var created = await _teacherRepository.CreateAsync(teacher);
            _logger.LogInformation("Teacher created — Id: {TeacherId} Name: {Name}", created.TeacherId, created.FullName);

            return CreatedAtAction(nameof(GetById), new { id = created.TeacherId }, new TeacherResponseDto
            {
                TeacherId = created.TeacherId,
                FullName  = created.FullName,
                Email     = created.Email,
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
            teacher.Subjects = dto.Subjects;
            teacher.IsActive = dto.IsActive;

            await _teacherRepository.UpdateAsync(teacher);
            _logger.LogInformation("Teacher updated — Id: {TeacherId}", id);

            return Ok(new TeacherResponseDto
            {
                TeacherId = teacher.TeacherId,
                FullName  = teacher.FullName,
                Email     = teacher.Email,
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
