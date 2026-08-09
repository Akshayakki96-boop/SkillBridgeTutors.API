using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IGoogleCalendarService
    {
        Task<string> CreateMeetingAsync(Lead lead, DemoBooking booking);
    }
}
