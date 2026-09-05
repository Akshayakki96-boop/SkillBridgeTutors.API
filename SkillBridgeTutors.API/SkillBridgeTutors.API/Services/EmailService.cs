using System.Net;
using Microsoft.Data.SqlClient;
using MimeKit;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailProvider _emailProvider;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IEmailProvider emailProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _emailProvider = emailProvider;
        }

        public async Task SendDemoConfirmationAsync(Lead lead, DemoBooking booking)
        {
            var fromEmail = _configuration["Email:From"] ?? "noreply@skillbridgetutors.com";

            var slot = booking.DemoSlot;
            var startTime = slot?.StartTime.ToString("dddd, dd MMMM yyyy") ?? "TBD";
            var timeRange = slot != null
                ? $"{slot.StartTime:HH:mm} – {slot.EndTime:HH:mm} UTC"
                : "TBD";
            var meetingLink = booking.MeetingLink ?? "#";

            var supportEmail = _configuration["Email:Support"] ?? "info@skillbridgetutors.com";
            var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
            var rescheduleLink = !string.IsNullOrWhiteSpace(frontendBaseUrl)
                ? $"{frontendBaseUrl}/demo/manage?bookingId={booking.BookingId}&action=reschedule"
                : BuildSupportMailto(
                    supportEmail,
                    $"Reschedule request - Booking #{booking.BookingId}",
                    $"Hi SkillBridge Team,\n\nI want to reschedule my demo booking.\nBooking ID: {booking.BookingId}\nName: {lead.FullName}\nEmail: {lead.Email}\n\nPlease share available slots.\n");

            var cancelLink = !string.IsNullOrWhiteSpace(frontendBaseUrl)
                ? $"{frontendBaseUrl}/demo/manage?bookingId={booking.BookingId}&action=cancel"
                : BuildSupportMailto(
                    supportEmail,
                    $"Cancel request - Booking #{booking.BookingId}",
                    $"Hi SkillBridge Team,\n\nI want to cancel my demo booking.\nBooking ID: {booking.BookingId}\nName: {lead.FullName}\nEmail: {lead.Email}\n\nReason: ");

            var emailSubject = "🎓 Your Free Demo Class is Confirmed – SkillBridge Tutors";

            var emailBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <title>Demo Confirmation</title>
</head>
<body style=""margin:0;padding:0;background-color:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;"">

  <!-- Wrapper -->
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6f9;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.08);"">

          <!-- Header -->
          <tr>
            <td style=""background:linear-gradient(135deg,#1a73e8,#0d47a1);padding:40px 40px 30px;text-align:center;"">
              <h1 style=""margin:0;color:#ffffff;font-size:28px;font-weight:700;letter-spacing:1px;"">
                SkillBridge Tutors
              </h1>
              <p style=""margin:8px 0 0;color:#bbdefb;font-size:14px;letter-spacing:2px;text-transform:uppercase;"">
                Excellence in Online Tutoring
              </p>
            </td>
          </tr>

          <!-- Success Badge -->
          <tr>
            <td align=""center"" style=""padding:30px 40px 0;"">
              <div style=""display:inline-block;background:#e8f5e9;border:2px solid #4caf50;border-radius:50px;padding:10px 28px;"">
                <span style=""color:#2e7d32;font-size:15px;font-weight:600;"">✅ &nbsp;Booking Confirmed</span>
              </div>
            </td>
          </tr>

          <!-- Greeting -->
          <tr>
            <td style=""padding:28px 40px 0;"">
              <h2 style=""margin:0;color:#1a1a2e;font-size:22px;font-weight:600;"">
                Hello, {lead.FullName}! 👋
              </h2>
              <p style=""margin:12px 0 0;color:#555;font-size:15px;line-height:1.7;"">
                We are delighted to confirm your <strong>Free Demo Class</strong> with SkillBridge Tutors. 
                Our expert tutor is looking forward to meeting you and your child!
              </p>
            </td>
          </tr>

          <!-- Booking Details Card -->
          <tr>
            <td style=""padding:28px 40px;"">
              <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                style=""background:#f0f4ff;border-radius:10px;border-left:5px solid #1a73e8;overflow:hidden;"">
                <tr>
                  <td style=""padding:24px 28px;"">
                    <p style=""margin:0 0 16px;color:#1a1a2e;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:1px;"">
                      📋 Booking Details
                    </p>

                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;"">
                          <span style=""color:#888;font-size:13px;"">Booking ID</span>
                        </td>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;text-align:right;"">
                          <strong style=""color:#1a1a2e;font-size:14px;"">#{booking.BookingId}</strong>
                        </td>
                      </tr>
                      <tr>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;"">
                          <span style=""color:#888;font-size:13px;"">Subject</span>
                        </td>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;text-align:right;"">
                          <strong style=""color:#1a1a2e;font-size:14px;"">{lead.Subject}</strong>
                        </td>
                      </tr>
                      <tr>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;"">
                          <span style=""color:#888;font-size:13px;"">📅 Date</span>
                        </td>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;text-align:right;"">
                          <strong style=""color:#1a1a2e;font-size:14px;"">{startTime}</strong>
                        </td>
                      </tr>
                      <tr>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;"">
                          <span style=""color:#888;font-size:13px;"">⏰ Time</span>
                        </td>
                        <td style=""padding:8px 0;border-bottom:1px solid #dce3f3;text-align:right;"">
                          <strong style=""color:#1a1a2e;font-size:14px;"">{timeRange}</strong>
                        </td>
                      </tr>
                      <tr>
                        <td style=""padding:8px 0;"">
                          <span style=""color:#888;font-size:13px;"">📧 Confirmation sent to</span>
                        </td>
                        <td style=""padding:8px 0;text-align:right;"">
                          <strong style=""color:#1a73e8;font-size:14px;"">{lead.Email}</strong>
                        </td>
                      </tr>
                    </table>

                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Join Button -->
          <tr>
            <td align=""center"" style=""padding:0 40px 32px;"">
              <a href=""{meetingLink}""
                style=""display:inline-block;background:linear-gradient(135deg,#1a73e8,#0d47a1);color:#ffffff;
                       text-decoration:none;padding:14px 40px;border-radius:50px;font-size:15px;
                       font-weight:600;letter-spacing:0.5px;box-shadow:0 4px 12px rgba(26,115,232,0.4);"">
                🎥 &nbsp; Join Demo Class
              </a>
              <p style=""margin:12px 0 0;color:#999;font-size:12px;"">
                (Meeting link will be active 10 minutes before the session)
              </p>
            </td>
          </tr>

          <!-- What to Expect -->
          <tr>
            <td style=""padding:0 40px 32px;"">
              <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                style=""background:#fff8e1;border-radius:10px;padding:20px 24px;"">
                <tr>
                  <td>
                    <p style=""margin:0 0 12px;color:#f57f17;font-size:13px;font-weight:700;text-transform:uppercase;"">
                      💡 What to Expect
                    </p>
                    <ul style=""margin:0;padding-left:18px;color:#555;font-size:14px;line-height:2;"">
                      <li>A personalised 1-to-1 session with an expert tutor</li>
                      <li>Assessment of your child's current level</li>
                      <li>Tailored learning plan discussion</li>
                      <li>Q&A — ask us anything!</li>
                    </ul>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Reschedule / Cancel -->
          <tr>
            <td align=""center"" style=""padding:0 40px 8px;"">
              <a href=""{rescheduleLink}"" style=""display:inline-block;background:#f4f7ff;color:#1a73e8;padding:12px 22px;border-radius:8px;text-decoration:none;font-size:14px;font-weight:600;margin-right:8px;border:1px solid #d8e3ff;"">Reschedule Demo</a>
              <a href=""{cancelLink}"" style=""display:inline-block;background:#fff2f2;color:#d32f2f;padding:12px 22px;border-radius:8px;text-decoration:none;font-size:14px;font-weight:600;border:1px solid #ffd2d2;"">Cancel Demo</a>
            </td>
          </tr>
          <tr>
            <td style=""padding:0 40px 32px;"">
              <p style=""margin:10px 0 0;color:#777;font-size:13px;line-height:1.8;text-align:center;"">
                If links don't open, email us at
                <a href=""mailto:{supportEmail}"" style=""color:#1a73e8;text-decoration:none;"">{supportEmail}</a>
                and share your Booking ID <strong>#{booking.BookingId}</strong>.
              </p>
            </td>
          </tr>

          <!-- Divider -->
          <tr>
            <td style=""padding:0 40px;"">
              <hr style=""border:none;border-top:1px solid #e8eaf6;margin:0;""/>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""padding:24px 40px;text-align:center;"">
              <p style=""margin:0;color:#aaa;font-size:12px;line-height:1.8;"">
                © 2026 SkillBridge Tutors. All rights reserved.<br/>
                <a href=""https://skillbridgetutors.com"" style=""color:#1a73e8;text-decoration:none;"">
                  www.skillbridgetutors.com
                </a>
                &nbsp;|&nbsp;
                <a href=""mailto:{supportEmail}"" style=""color:#1a73e8;text-decoration:none;"">
                  {supportEmail}
                </a>
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>

</body>
</html>";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("SkillBridge Tutors", fromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(lead.Email));
            mimeMessage.Subject = emailSubject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailBody };

            // --- Send email ---
            string sendError = string.Empty;
            string providerMessageId = string.Empty;
            try
            {
                providerMessageId = await _emailProvider.SendEmailAsync(mimeMessage);
                _logger.LogInformation("Confirmation email sent to {Email}", lead.Email);
            }
            catch (Exception ex)
            {
                var inner = ex;
                var sb = new System.Text.StringBuilder();
                while (inner != null) { sb.AppendLine(inner.Message); inner = inner.InnerException; }
                sendError = sb.ToString().Trim();
                _logger.LogError(ex, "Failed to send confirmation email to {Email}: {FullError}", lead.Email, sendError);
            }

            // --- Write log row via raw ADO.NET (bypasses EF tracking) ---
            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection")!;
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO EmailLogs (LeadId, BookingId, ToEmail, Subject, EmailType, Status, ProviderMessageId, ErrorMessage, SentAt, CreatedAt)
                    VALUES (@LeadId, @BookingId, @ToEmail, @Subject, @EmailType, @Status, @ProviderMessageId, @ErrorMessage, @SentAt, @CreatedAt)";
                cmd.Parameters.AddWithValue("@LeadId",              (object?)lead.LeadId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BookingId",           (object?)booking.BookingId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToEmail",             lead.Email);
                cmd.Parameters.AddWithValue("@Subject",             emailSubject);
                cmd.Parameters.AddWithValue("@EmailType",           "DemoConfirmation");
                cmd.Parameters.AddWithValue("@Status",              string.IsNullOrEmpty(sendError) ? "Sent" : "Failed");
                cmd.Parameters.AddWithValue("@ProviderMessageId",   string.IsNullOrEmpty(providerMessageId) ? DBNull.Value : (object)providerMessageId);
                cmd.Parameters.AddWithValue("@ErrorMessage",        string.IsNullOrEmpty(sendError) ? DBNull.Value : (object)sendError);
                cmd.Parameters.AddWithValue("@SentAt",              DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@CreatedAt",           DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("EmailLog written for booking {BookingId} status={Status}", booking.BookingId, string.IsNullOrEmpty(sendError) ? "Sent" : "Failed");
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to write EmailLog for booking {BookingId}: {Msg}", booking.BookingId, logEx.Message);
            }

            if (!string.IsNullOrEmpty(sendError))
                throw new InvalidOperationException(sendError);
        }

        public async Task SendStudentRegistrationAsync(Student student)
        {
            var fromEmail = _configuration["Email:From"] ?? "noreply@skillbridgetutors.com";

            var frontendBaseUrl = _configuration["Frontend:BaseUrl"]?.TrimEnd('/');
            var loginLink = !string.IsNullOrWhiteSpace(frontendBaseUrl)
                ? $"{frontendBaseUrl}/student/login"
                : "#";

            var supportEmail = _configuration["Email:Support"] ?? "info@skillbridgetutors.com";

            var subject = "Welcome to SkillBridge Tutors — Your account is ready";

            var body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <title>Welcome to SkillBridge Tutors</title>
</head>
<body style=""margin:0;padding:0;background-color:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;"">

  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6f9;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#1a73e8,#0d47a1);padding:40px 40px 30px;text-align:center;"">
              <h1 style=""margin:0;color:#ffffff;font-size:28px;font-weight:700;letter-spacing:1px;"">SkillBridge Tutors</h1>
              <p style=""margin:8px 0 0;color:#bbdefb;font-size:14px;letter-spacing:2px;text-transform:uppercase;"">Welcome to SkillBridge</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:28px 40px 0;"">
              <h2 style=""margin:0;color:#1a1a2e;font-size:22px;font-weight:600;"">Hello, {student.FullName} 👋</h2>
              <p style=""margin:12px 0 0;color:#555;font-size:15px;line-height:1.7;"">Your student account has been created. Use the link below to login.</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:20px 40px;"">
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f0f4ff;border-radius:10px;border-left:5px solid #1a73e8;overflow:hidden;"">
                <tr>
                  <td style=""padding:16px 20px;"">
                    <p style=""margin:0;color:#888;font-size:13px;""><strong>Login Email:</strong> {student.Email}</p>
                    <p style=""margin:6px 0 0;color:#888;font-size:13px;""><strong>Phone:</strong> {student.Phone}</p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:0 40px 32px;"">
              <a href=""{loginLink}""
                style=""display:inline-block;background:linear-gradient(135deg,#1a73e8,#0d47a1);color:#ffffff;text-decoration:none;padding:14px 40px;border-radius:50px;font-size:15px;font-weight:600;letter-spacing:0.5px;box-shadow:0 4px 12px rgba(26,115,232,0.4);"">
                🔐 &nbsp; Login to your account
              </a>
            </td>
          </tr>
          <tr>
            <td style=""padding:0 40px 32px;"">
              <p style=""margin:10px 0 0;color:#777;font-size:13px;line-height:1.8;text-align:center;"">
                If you forgot your password, use the Forgot Password flow on the login page to reset it. For assistance, email us at
                <a href=""mailto:{supportEmail}"" style=""color:#1a73e8;text-decoration:none;"">{supportEmail}</a>
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:24px 40px;text-align:center;"">
              <p style=""margin:0;color:#aaa;font-size:12px;line-height:1.8;"">© SkillBridge Tutors. All rights reserved.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("SkillBridge Tutors", fromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(student.Email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            string sendError = string.Empty;
            string providerMessageId = string.Empty;
            try
            {
                providerMessageId = await _emailProvider.SendEmailAsync(mimeMessage);
                _logger.LogInformation("Student registration email sent to {Email}", student.Email);
            }
            catch (Exception ex)
            {
                var inner = ex;
                var sb = new System.Text.StringBuilder();
                while (inner != null) { sb.AppendLine(inner.Message); inner = inner.InnerException; }
                sendError = sb.ToString().Trim();
                _logger.LogError(ex, "Failed to send registration email to {Email}: {FullError}", student.Email, sendError);
            }

            // Log to EmailLogs table
            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection")!;
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO EmailLogs (LeadId, BookingId, ToEmail, Subject, EmailType, Status, ProviderMessageId, ErrorMessage, SentAt, CreatedAt)
                    VALUES (@LeadId, @BookingId, @ToEmail, @Subject, @EmailType, @Status, @ProviderMessageId, @ErrorMessage, @SentAt, @CreatedAt)";
                cmd.Parameters.AddWithValue("@LeadId", DBNull.Value);
                cmd.Parameters.AddWithValue("@BookingId", DBNull.Value);
                cmd.Parameters.AddWithValue("@ToEmail", student.Email);
                cmd.Parameters.AddWithValue("@Subject", subject);
                cmd.Parameters.AddWithValue("@EmailType", "StudentRegistration");
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(sendError) ? "Sent" : "Failed");
                cmd.Parameters.AddWithValue("@ProviderMessageId", string.IsNullOrEmpty(providerMessageId) ? DBNull.Value : (object)providerMessageId);
                cmd.Parameters.AddWithValue("@ErrorMessage", string.IsNullOrEmpty(sendError) ? DBNull.Value : (object)sendError);
                cmd.Parameters.AddWithValue("@SentAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("EmailLog written for student {Email} status={Status}", student.Email, string.IsNullOrEmpty(sendError) ? "Sent" : "Failed");
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to write EmailLog for student {Email}: {Msg}", student.Email, logEx.Message);
            }

            if (!string.IsNullOrEmpty(sendError))
                throw new InvalidOperationException(sendError);
        }

        public async Task SendStudentPasswordResetAsync(Student student, string resetLink)
        {
            var fromEmail = _configuration["Email:From"] ?? "noreply@skillbridgetutors.com";
            var supportEmail = _configuration["Email:Support"] ?? "info@skillbridgetutors.com";
            var subject = "Reset your SkillBridge Tutors password";

            var body = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0""/>
  <title>Password Reset</title>
</head>
<body style=""margin:0;padding:0;background-color:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6f9;padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.08);"">
          <tr>
            <td style=""background:linear-gradient(135deg,#1a73e8,#0d47a1);padding:40px 40px 30px;text-align:center;"">
              <h1 style=""margin:0;color:#ffffff;font-size:28px;font-weight:700;letter-spacing:1px;"">SkillBridge Tutors</h1>
              <p style=""margin:8px 0 0;color:#bbdefb;font-size:14px;letter-spacing:2px;text-transform:uppercase;"">Password reset request</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:28px 40px 0;"">
              <h2 style=""margin:0;color:#1a1a2e;font-size:22px;font-weight:600;"">Hello, {student.FullName} 👋</h2>
              <p style=""margin:12px 0 0;color:#555;font-size:15px;line-height:1.7;"">We received a request to reset your password. Click the button below. This link expires in 30 minutes.</p>
            </td>
          </tr>
          <tr>
            <td align=""center"" style=""padding:24px 40px 32px;"">
              <a href=""{resetLink}""
                 style=""display:inline-block;background:linear-gradient(135deg,#1a73e8,#0d47a1);color:#ffffff;text-decoration:none;padding:14px 40px;border-radius:50px;font-size:15px;font-weight:600;letter-spacing:0.5px;box-shadow:0 4px 12px rgba(26,115,232,0.4);"">
                Reset Password
              </a>
            </td>
          </tr>
          <tr>
            <td style=""padding:0 40px 32px;"">
              <p style=""margin:0;color:#777;font-size:13px;line-height:1.8;text-align:center;"">If you did not request this, please ignore this email. Need help? Contact <a href=""mailto:{supportEmail}"" style=""color:#1a73e8;text-decoration:none;"">{supportEmail}</a>.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("SkillBridge Tutors", fromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(student.Email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            string sendError = string.Empty;
            string providerMessageId = string.Empty;
            try
            {
                providerMessageId = await _emailProvider.SendEmailAsync(mimeMessage);
                _logger.LogInformation("Student password reset email sent to {Email}", student.Email);
            }
            catch (Exception ex)
            {
                var inner = ex;
                var sb = new System.Text.StringBuilder();
                while (inner != null) { sb.AppendLine(inner.Message); inner = inner.InnerException; }
                sendError = sb.ToString().Trim();
                _logger.LogError(ex, "Failed to send student reset email to {Email}: {FullError}", student.Email, sendError);
            }

            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection")!;
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO EmailLogs (LeadId, BookingId, ToEmail, Subject, EmailType, Status, ProviderMessageId, ErrorMessage, SentAt, CreatedAt)
                    VALUES (@LeadId, @BookingId, @ToEmail, @Subject, @EmailType, @Status, @ProviderMessageId, @ErrorMessage, @SentAt, @CreatedAt)";
                cmd.Parameters.AddWithValue("@LeadId", DBNull.Value);
                cmd.Parameters.AddWithValue("@BookingId", DBNull.Value);
                cmd.Parameters.AddWithValue("@ToEmail", student.Email);
                cmd.Parameters.AddWithValue("@Subject", subject);
                cmd.Parameters.AddWithValue("@EmailType", "StudentPasswordReset");
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(sendError) ? "Sent" : "Failed");
                cmd.Parameters.AddWithValue("@ProviderMessageId", string.IsNullOrEmpty(providerMessageId) ? DBNull.Value : (object)providerMessageId);
                cmd.Parameters.AddWithValue("@ErrorMessage", string.IsNullOrEmpty(sendError) ? DBNull.Value : (object)sendError);
                cmd.Parameters.AddWithValue("@SentAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to write EmailLog for student reset email {Email}: {Msg}", student.Email, logEx.Message);
            }

            if (!string.IsNullOrEmpty(sendError))
                throw new InvalidOperationException(sendError);
        }

        public async Task SendTeacherNotificationAsync(Teacher teacher, Lead lead, DemoBooking booking)
        {
            var fromEmail = _configuration["Email:From"] ?? "noreply@skillbridgetutors.com";

            var slot = booking.DemoSlot;
            var startTime = slot?.StartTime.ToString("dddd, dd MMMM yyyy") ?? "TBD";
            var timeRange = slot != null
                ? $"{slot.StartTime:HH:mm} – {slot.EndTime:HH:mm} UTC"
                : "TBD";
            var meetingLink = booking.MeetingLink ?? "#";

            var body = $@"
<!DOCTYPE html>
<html>
<body style=""font-family:'Segoe UI',Arial,sans-serif;background:#f4f6f9;padding:30px;"">
  <table width=""600"" style=""background:#fff;border-radius:10px;padding:30px;margin:auto;box-shadow:0 4px 12px rgba(0,0,0,0.08);"">
    <tr><td style=""background:linear-gradient(135deg,#1a73e8,#0d47a1);padding:30px;border-radius:8px 8px 0 0;text-align:center;"">
      <h1 style=""color:#fff;margin:0;"">SkillBridge Tutors</h1>
      <p style=""color:#bbdefb;margin:6px 0 0;"">New Demo Class Assigned</p>
    </td></tr>
    <tr><td style=""padding:28px;"">
      <h2 style=""color:#1a1a2e;"">Hello, {teacher.FullName}! 👋</h2>
      <p style=""color:#555;font-size:15px;"">You have been assigned a <strong>Free Demo Class</strong>. Please be available at the scheduled time.</p>

      <table width=""100%"" style=""background:#f0f4ff;border-left:5px solid #1a73e8;border-radius:8px;padding:20px;margin-top:16px;"">
        <tr><td style=""padding:8px 0;""><strong>Student:</strong> {lead.StudentName}</td></tr>
        <tr><td style=""padding:8px 0;""><strong>Subject:</strong> {lead.Subject}</td></tr>
        <tr><td style=""padding:8px 0;""><strong>Date:</strong> {startTime}</td></tr>
        <tr><td style=""padding:8px 0;""><strong>Time:</strong> {timeRange}</td></tr>
        <tr><td style=""padding:8px 0;""><strong>Parent Email:</strong> {lead.Email}</td></tr>
        <tr><td style=""padding:8px 0;""><strong>Parent Phone:</strong> {lead.Phone}</td></tr>
        <tr><td style=""padding:8px 0;""><strong>Query:</strong> {lead.Query}</td></tr>
      </table>

      <div style=""text-align:center;margin-top:28px;"">
        <a href=""{meetingLink}"" style=""background:#1a73e8;color:#fff;padding:14px 32px;border-radius:6px;text-decoration:none;font-size:15px;font-weight:600;"">
          Join Demo Class
        </a>
      </div>

      <p style=""color:#888;font-size:13px;margin-top:24px;text-align:center;"">
        Please join the session 5 minutes early to set up.
      </p>
    </td></tr>
  </table>
</body>
</html>";

            var teacherSubject = $"📚 Demo Class Assigned – {lead.FullName} ({lead.Subject}) on {startTime}";
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("SkillBridge Tutors", fromEmail));
            mimeMessage.To.Add(MailboxAddress.Parse(teacher.Email));
            mimeMessage.Subject = teacherSubject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            // --- Send email ---
            string sendError = string.Empty;
            string providerMessageId = string.Empty;
            try
            {
                providerMessageId = await _emailProvider.SendEmailAsync(mimeMessage);
                _logger.LogInformation("Teacher notification email sent to {Email}", teacher.Email);
            }
            catch (Exception ex)
            {
                var inner = ex;
                var sb = new System.Text.StringBuilder();
                while (inner != null) { sb.AppendLine(inner.Message); inner = inner.InnerException; }
                sendError = sb.ToString().Trim();
                _logger.LogError(ex, "Failed to send teacher notification email to {Email}: {FullError}", teacher.Email, sendError);
            }

            // --- Write log row via raw ADO.NET (bypasses EF tracking) ---
            try
            {
                var connStr = _configuration.GetConnectionString("DefaultConnection")!;
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO EmailLogs (LeadId, BookingId, ToEmail, Subject, EmailType, Status, ProviderMessageId, ErrorMessage, SentAt, CreatedAt)
                    VALUES (@LeadId, @BookingId, @ToEmail, @Subject, @EmailType, @Status, @ProviderMessageId, @ErrorMessage, @SentAt, @CreatedAt)";
                cmd.Parameters.AddWithValue("@LeadId",              (object?)lead.LeadId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@BookingId",           (object?)booking.BookingId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToEmail",             teacher.Email);
                cmd.Parameters.AddWithValue("@Subject",             teacherSubject);
                cmd.Parameters.AddWithValue("@EmailType",           "Other");
                cmd.Parameters.AddWithValue("@Status",              string.IsNullOrEmpty(sendError) ? "Sent" : "Failed");
                cmd.Parameters.AddWithValue("@ProviderMessageId",   string.IsNullOrEmpty(providerMessageId) ? DBNull.Value : (object)providerMessageId);
                cmd.Parameters.AddWithValue("@ErrorMessage",        string.IsNullOrEmpty(sendError) ? DBNull.Value : (object)sendError);
                cmd.Parameters.AddWithValue("@SentAt",              DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@CreatedAt",           DateTime.UtcNow);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("EmailLog written for booking {BookingId} status={Status}", booking.BookingId, string.IsNullOrEmpty(sendError) ? "Sent" : "Failed");
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to write EmailLog for booking {BookingId}: {Msg}", booking.BookingId, logEx.Message);
            }

            if (!string.IsNullOrEmpty(sendError))
                throw new InvalidOperationException(sendError);
        }

        private static string BuildSupportMailto(string supportEmail, string subject, string body)
        {
            var encodedSubject = Uri.EscapeDataString(subject);
            var encodedBody = Uri.EscapeDataString(body);
            return $"mailto:{supportEmail}?subject={encodedSubject}&body={encodedBody}";
        }
    }
}
