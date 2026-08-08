using System.Net;
using System.Net.Mail;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendDemoConfirmationAsync(Lead lead, DemoBooking booking)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPass"];
            var fromEmail = _configuration["Email:From"] ?? smtpUser;

            var slot = booking.DemoSlot;
            var slotTime = slot?.StartTime.ToString("dddd, dd MMMM yyyy 'at' HH:mm 'UTC'") ?? "TBD";
            var meetingLink = booking.MeetingLink ?? "A meeting link will be sent shortly.";

            var subject = "SkillBridge Tutors - Your Free Demo Class is Confirmed!";
            var body = $@"
Dear {lead.FullName},

Thank you for booking a free demo class with SkillBridge Tutors.

Here are your booking details:

  Subject       : {lead.Subject}
  Date & Time   : {slotTime}
  Meeting Link  : {meetingLink}

If you need to reschedule or cancel, please contact us and we will be happy to help.

Warm regards,
SkillBridge Tutors Team
";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(fromEmail!, lead.Email, subject, body);

            try
            {
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Confirmation email sent to {Email}", lead.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", lead.Email);
                throw;
            }
        }
    }
}
