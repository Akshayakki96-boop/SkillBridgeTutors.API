using MimeKit;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IEmailProvider
    {
        /// <summary>
        /// Sends an email and returns the provider message ID if successful
        /// </summary>
        /// <returns>Provider message ID or empty string if failed</returns>
        Task<string> SendEmailAsync(MimeMessage mimeMessage);
    }
}
