using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/teacher/auth")]
    public class TeacherAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TeacherAuthController> _logger;

        public TeacherAuthController(
            ApplicationDbContext context,
            ITokenService tokenService,
            ILogger<TeacherAuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Teacher login with email and password.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TeacherLoginDto dto)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == dto.Email);

            if (teacher == null || string.IsNullOrWhiteSpace(teacher.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, teacher.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            if (!teacher.IsActive)
                return Unauthorized(new { message = "This account has been deactivated." });

            var token = _tokenService.GenerateToken(teacher);

            _logger.LogInformation("Teacher logged in — Id: {TeacherId} Email: {Email}", teacher.TeacherId, teacher.Email);

            return Ok(new TeacherAuthResponseDto
            {
                Token = token,
                TeacherId = teacher.TeacherId,
                Email = teacher.Email,
                FullName = teacher.FullName,
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            });
        }

        /// <summary>
        /// Submit a teacher application to join SkillBridge.
        /// </summary>
        [HttpPost("apply")]
        [AllowAnonymous]
        public async Task<IActionResult> ApplyForTeacher([FromBody] TeacherApplicationDto dto)
        {
            // Check if email already registered
            var existingTeacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Email == dto.Email);
            if (existingTeacher != null)
                return Conflict(new { message = "This email is already registered as a teacher." });

            // Create a teacher application/record with no password initially
            // Admin will set password later and activate
            var teacher = new Teacher
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Subjects = dto.Subjects,
                PasswordHash = null, // No password yet - admin will set it
                IsActive = false // Not active until approved by admin
            };

            await _context.Teachers.AddAsync(teacher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Teacher application submitted — Email: {Email} Name: {FullName}", dto.Email, dto.FullName);

            return CreatedAtAction(nameof(Login), new
            {
                message = "Application submitted successfully. Our team will review and contact you soon.",
                teacherId = teacher.TeacherId,
                email = teacher.Email
            });
        }
    }
}
