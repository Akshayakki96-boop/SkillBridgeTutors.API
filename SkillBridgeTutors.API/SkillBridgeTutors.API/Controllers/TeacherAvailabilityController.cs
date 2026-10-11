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
    [Route("api/teachers/availability")]
    [Authorize(Roles = "Teacher")]
    public class TeacherAvailabilityController : ControllerBase
    {
        private readonly ITeacherAvailabilityRepository _availabilityRepository;

        public TeacherAvailabilityController(ITeacherAvailabilityRepository availabilityRepository)
        {
            _availabilityRepository = availabilityRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAvailability()
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid teacher token." });

            var slots = await _availabilityRepository.GetByTeacherAsync(teacherId);
            return Ok(slots.Select(ToDto));
        }

        [HttpPost]
        public async Task<IActionResult> AddAvailability([FromBody] CreateTeacherAvailabilityDto dto)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid teacher token." });

            if (dto.EndTime <= dto.StartTime)
                return BadRequest(new { message = "End time must be after start time." });

            var slot = await _availabilityRepository.CreateAsync(new TeacherAvailability
            {
                TeacherId = teacherId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            });

            return Ok(ToDto(slot));
        }

        [HttpDelete("{availabilityId:long}")]
        public async Task<IActionResult> RemoveAvailability(long availabilityId)
        {
            if (!TryGetTeacherId(out var teacherId))
                return Unauthorized(new { message = "Invalid teacher token." });

            var slot = await _availabilityRepository.GetByIdAsync(availabilityId);
            if (slot == null || slot.TeacherId != teacherId)
                return NotFound(new { message = "Availability slot not found." });

            await _availabilityRepository.DeleteAsync(slot);
            return NoContent();
        }

        private bool TryGetTeacherId(out long teacherId)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return long.TryParse(claim, out teacherId);
        }

        private static TeacherAvailabilityDto ToDto(TeacherAvailability slot) => new()
        {
            AvailabilityId = slot.AvailabilityId,
            DayOfWeek = slot.DayOfWeek,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime
        };
    }
}
