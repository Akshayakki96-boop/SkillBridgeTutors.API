using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleCalendarService> _logger;

        public GoogleCalendarService(IConfiguration configuration, ILogger<GoogleCalendarService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> CreateMeetingAsync(Lead lead, DemoBooking booking)
        {
            var calendarId = _configuration["Google:CalendarId"];
            var credentialsJson = _configuration["Google:CredentialsJson"];

            // Load service account credentials from JSON string
            GoogleCredential credential;
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(credentialsJson!)))
            {
                credential = GoogleCredential
                    .FromStream(stream)
                    .CreateScoped(CalendarService.Scope.Calendar);
            }

            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "SkillBridge Tutors"
            });

            var slot = booking.DemoSlot;

            var calendarEvent = new Event
            {
                Summary = $"Free Demo Class – {lead.FullName} ({lead.Subject})",
                Description = $"SkillBridge Tutors Free Demo Class\n\nParent: {lead.FullName}\nEmail: {lead.Email}\nPhone: {lead.Phone}\nSubject: {lead.Subject}\nQuery: {lead.Query}",
                Start = new EventDateTime
                {
                    DateTimeDateTimeOffset = slot.StartTime,
                    TimeZone = "UTC"
                },
                End = new EventDateTime
                {
                    DateTimeDateTimeOffset = slot.EndTime,
                    TimeZone = "UTC"
                },
                Attendees = new List<EventAttendee>
                {
                    new EventAttendee { Email = lead.Email, DisplayName = lead.FullName }
                },
                ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey
                        {
                            Type = "hangoutsMeet"
                        }
                    }
                },
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new EventReminder { Method = "email", Minutes = 1440 }, // 24 hours
                        new EventReminder { Method = "popup", Minutes = 30 }    // 30 minutes
                    }
                }
            };

            var request = service.Events.Insert(calendarEvent, calendarId);
            request.ConferenceDataVersion = 1;
            request.SendUpdates = EventsResource.InsertRequest.SendUpdatesEnum.All;

            var createdEvent = await request.ExecuteAsync();

            var meetLink = createdEvent.ConferenceData?.EntryPoints?
                .FirstOrDefault(e => e.EntryPointType == "video")?.Uri
                ?? createdEvent.HangoutLink
                ?? string.Empty;

            _logger.LogInformation("Google Meet created: {MeetLink} for lead {LeadId}", meetLink, lead.LeadId);

            return meetLink;
        }
    }
}
