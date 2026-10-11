using Google.Apis.Auth.OAuth2;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly ILogger<GoogleCalendarService> _logger;

        public GoogleCalendarService(ILogger<GoogleCalendarService> logger)
        {
            _logger = logger;
        }

        public Task<string> CreateMeetingAsync(Lead lead, DemoBooking booking)
        {
            var roomName = $"skillbridge-{Guid.NewGuid():N}";
            var meetLink = $"https://meet.jit.si/{roomName}";

            _logger.LogInformation("Jitsi Meet link created: '{MeetLink}' for lead {LeadId}", meetLink, lead.LeadId);

            return Task.FromResult(meetLink);
        }

        public Task<string> CreateClassMeetingAsync()
        {
            var roomName = $"skillbridge-class-{Guid.NewGuid():N}";
            var meetLink = $"https://meet.jit.si/{roomName}";

            _logger.LogInformation("Jitsi Meet link created for regular class: '{MeetLink}'", meetLink);

            return Task.FromResult(meetLink);
        }
    }
}
