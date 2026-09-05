using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IEmailService
    {
        Task SendDemoConfirmationAsync(Lead lead, DemoBooking booking);
        Task SendTeacherNotificationAsync(Teacher teacher, Lead lead, DemoBooking booking);
        Task SendStudentRegistrationAsync(Student student);
        Task SendStudentPasswordResetAsync(Student student, string resetLink);
    }
}
