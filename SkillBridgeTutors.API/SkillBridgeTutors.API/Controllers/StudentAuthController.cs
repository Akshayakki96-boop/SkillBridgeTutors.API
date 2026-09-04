using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/student/auth")]
    public class StudentAuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public StudentAuthController(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Register a new student.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] StudentRegisterDto dto)
        {
            var exists = await _context.Students.AnyAsync(s => s.Email == dto.Email);
            if (exists)
                return Conflict(new { message = "An account with this email already exists." });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var student = new Student
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Phone = dto.Phone,
                ParentFirstName = dto.ParentFirstName,
                ParentLastName = dto.ParentLastName,
                ClassYear = dto.ClassYear,
                Subject = dto.Subject,
                Address = dto.Address
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(student);

            return CreatedAtAction(nameof(Register), new StudentAuthResponseDto
            {
                Token = token,
                StudentId = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            });
        }

        /// <summary>
        /// Login and receive a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] StudentLoginDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == dto.Email);
            if (student == null || !BCrypt.Net.BCrypt.Verify(dto.Password, student.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            if (!student.IsActive)
                return Unauthorized(new { message = "This account has been deactivated." });

            var token = _tokenService.GenerateToken(student);

            return Ok(new StudentAuthResponseDto
            {
                Token = token,
                StudentId = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            });
        }
    }
}
