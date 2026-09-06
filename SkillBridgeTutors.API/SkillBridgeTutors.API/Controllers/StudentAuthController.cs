using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public StudentAuthController(
            ApplicationDbContext context,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
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
                Address = dto.Address,
                IsActive = false
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new StudentAuthResponseDto
            {
                StudentId = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                ExpiresAt = null,
                PaymentRequired = true,
                PaymentCompleted = false
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
                return Unauthorized(new { message = "Please complete payment to activate your account." });

            var token = _tokenService.GenerateToken(student);

            return Ok(new StudentAuthResponseDto
            {
                Token = token,
                StudentId = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                ExpiresAt = DateTime.UtcNow.AddHours(12),
                PaymentRequired = true,
                PaymentCompleted = true
            });
        }

        /// <summary>
        /// Sends password reset link to student email if account exists.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] StudentForgotPasswordDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == dto.Email);

            if (student != null && student.IsActive)
            {
                var resetToken = _tokenService.GenerateStudentPasswordResetToken(student);

                var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
                var resetLink = !string.IsNullOrWhiteSpace(frontendBaseUrl)
                    ? $"{frontendBaseUrl}/student/reset-password?token={Uri.EscapeDataString(resetToken)}"
                    : $"{Request.Scheme}://{Request.Host}/student/reset-password?token={Uri.EscapeDataString(resetToken)}";

                try
                {
                    await _emailService.SendStudentPasswordResetAsync(student, resetLink);
                }
                catch
                {
                    // Email errors are logged in EmailService.
                }
            }

            return Ok(new { message = "If an account exists with this email, a reset link has been sent." });
        }

        /// <summary>
        /// Resets student password using a valid reset token.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] StudentResetPasswordDto dto)
        {
            Student? student = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                var studentIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(studentIdClaim, out var authenticatedStudentId))
                {
                    student = await _context.Students.FirstOrDefaultAsync(s => s.Id == authenticatedStudentId);
                }
            }

            if (student == null)
            {
                if (!_tokenService.TryGetStudentIdFromPasswordResetToken(dto.Token ?? string.Empty, out var studentId))
                    return BadRequest(new { message = "Invalid or expired reset token." });

                student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            }

            if (student == null || !student.IsActive)
                return BadRequest(new { message = "Invalid reset request." });

            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}
