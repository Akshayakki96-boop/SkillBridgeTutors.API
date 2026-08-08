using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IEmailService
    {
        Task SendDemoConfirmationAsync(Lead lead, DemoBooking booking);
    }
}
