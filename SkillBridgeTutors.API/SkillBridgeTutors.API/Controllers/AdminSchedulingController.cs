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

        public AdminSchedulingController(IClassSchedulingService schedulingService)
        {
            _schedulingService = schedulingService;
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
    }
}
