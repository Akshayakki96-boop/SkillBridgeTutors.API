using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;
using SkillBridgeTutors.API.Services;

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
        private readonly IGoogleAuthService _googleAuthService;

        public StudentAuthController(
            ApplicationDbContext context,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration,
            IGoogleAuthService googleAuthService)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _googleAuthService = googleAuthService;
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
                AuthProvider = "email",
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

            // Allow login regardless of payment status
            // Payment is checked separately for feature access (class schedule, etc.)
            var token = _tokenService.GenerateToken(student);

            return Ok(new StudentAuthResponseDto
            {
                Token = token,
                StudentId = student.Id,
                Email = student.Email,
                FullName = student.FullName,
                ExpiresAt = DateTime.UtcNow.AddHours(12),
                PaymentRequired = true,
                PaymentCompleted = student.IsActive  // IsActive indicates payment completion
            });
        }

        /// <summary>
        /// Sign up or login with Google
        /// </summary>
        [HttpPost("google-signin")]
        public async Task<IActionResult> GoogleSignIn([FromBody] StudentGoogleSignInDto dto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(dto.IdToken))
                    return BadRequest(new { message = "IdToken is required." });

                // Verify the ID token
                var payload = await _googleAuthService.VerifyIdTokenAsync(dto.IdToken);
                if (payload == null)
                    return Unauthorized(new { message = "Invalid Google token." });

                // Validate email from payload
                if (string.IsNullOrWhiteSpace(payload.Email))
                    return BadRequest(new { message = "Google account email is required." });

                // Check if student exists by Google ID
                var student = await _context.Students.FirstOrDefaultAsync(s => s.GoogleId == payload.Subject);

                if (student != null)
                {
                    // Existing student - login allowed regardless of payment status
                    // Payment status is checked separately for feature access
                    var authToken = _tokenService.GenerateToken(student);
                    return Ok(new StudentAuthResponseDto
                    {
                        Token = authToken,
                        StudentId = student.Id,
                        Email = student.Email,
                        FullName = student.FullName,
                        ExpiresAt = DateTime.UtcNow.AddHours(12),
                        PaymentRequired = true,
                        PaymentCompleted = student.IsActive  // IsActive indicates payment completion
                    });
                }

                // New student - check if email already exists
                var existingByEmail = await _context.Students.FirstOrDefaultAsync(s => s.Email == payload.Email);
                if (existingByEmail != null)
                {
                    // Email exists but not linked to Google - offer to link
                    return Conflict(new
                    {
                        message = "Email already registered. Please login with your password or reset it.",
                        email = payload.Email
                    });
                }

                // Create new student
                var newStudent = new Student
                {
                    FullName = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    GoogleId = payload.Subject,
                    AuthProvider = "google",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password - not used
                    Phone = dto.Phone ?? string.Empty,
                    ParentFirstName = dto.ParentFirstName,
                    ParentLastName = dto.ParentLastName,
                    ClassYear = dto.ClassYear,
                    Subject = dto.Subject,
                    Address = dto.Address,
                    IsActive = true  // Allow login even without payment
                };

                _context.Students.Add(newStudent);
                await _context.SaveChangesAsync();

                // Generate token for new user
                var token = _tokenService.GenerateToken(newStudent);

                return CreatedAtAction(nameof(GoogleSignIn), new StudentAuthResponseDto
                {
                    Token = token,  // Provide token so user can login
                    StudentId = newStudent.Id,
                    Email = newStudent.Email,
                    FullName = newStudent.FullName,
                    ExpiresAt = DateTime.UtcNow.AddHours(12),
                    PaymentRequired = true,
                    PaymentCompleted = false  // Flag that payment is still needed
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "An error occurred during Google sign-in.",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Link Google account to existing student (optional enhancement)
        /// </summary>
        [HttpPost("link-google")]
        public async Task<IActionResult> LinkGoogleAccount([FromBody] GoogleSignInDto dto)
        {
            // Verify Google token
            var payload = await _googleAuthService.VerifyIdTokenAsync(dto.IdToken);
            if (payload == null)
                return Unauthorized(new { message = "Invalid Google token." });

            // Get authenticated user
            var studentIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(studentIdClaim, out var studentId))
                return Unauthorized(new { message = "Invalid token." });

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null)
                return NotFound();

            // Check if Google ID is already linked to another account
            var existingGoogleUser = await _context.Students.FirstOrDefaultAsync(s => s.GoogleId == payload.Subject);
            if (existingGoogleUser != null && existingGoogleUser.Id != studentId)
                return Conflict(new { message = "This Google account is already linked to another user." });

            // Link the Google account
            student.GoogleId = payload.Subject;
            if (string.IsNullOrEmpty(student.AuthProvider))
                student.AuthProvider = "email+google";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Google account linked successfully." });
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
