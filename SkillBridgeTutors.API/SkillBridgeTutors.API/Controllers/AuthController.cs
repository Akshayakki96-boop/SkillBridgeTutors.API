using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Register a new admin user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var exists = await _context.AdminUsers.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return Conflict(new { message = "An account with this email already exists." });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new AdminUser
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash
            };

            _context.AdminUsers.Add(user);
            await _context.SaveChangesAsync();

            var token = _tokenService.GenerateToken(user);

            return CreatedAtAction(nameof(Register), new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            });
        }

        /// <summary>
        /// Login and receive a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _tokenService.GenerateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                FullName = user.FullName,
                ExpiresAt = DateTime.UtcNow.AddHours(12)
            });
        }
    }
}
