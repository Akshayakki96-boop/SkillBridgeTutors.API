using MimeKit;
using SkillBridgeTutors.API.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkillBridgeTutors.API.Services
{
    public class ResendEmailProvider : IEmailProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResendEmailProvider> _logger;
        private readonly HttpClient _httpClient;
        private const string ResendApiUrl = "https://api.resend.com/emails";

        public ResendEmailProvider(IConfiguration configuration, ILogger<ResendEmailProvider> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<string> SendEmailAsync(MimeMessage mimeMessage)
        {
            var resendApiKey = _configuration["Email:ResendApiKey"];
            if (string.IsNullOrEmpty(resendApiKey))
            {
                _logger.LogError("Resend API key is not configured");
                throw new InvalidOperationException("Resend API key is not configured (Email:ResendApiKey)");
            }

            // Log key details for debugging
            _logger.LogInformation("Resend API Key length: {Length}", resendApiKey.Length);
            _logger.LogInformation("Resend API Key starts with: {Start}", resendApiKey.Substring(0, Math.Min(15, resendApiKey.Length)));

            try
            {
                // Extract email details from MimeMessage
                if (mimeMessage.To.Count == 0 || mimeMessage.From.Count == 0)
                {
                    throw new InvalidOperationException("Email must have both 'To' and 'From' addresses");
                }

                var toAddress = (mimeMessage.To.First() as MailboxAddress)?.Address;
                var subject = mimeMessage.Subject ?? "No Subject";
                var fromAddress = (mimeMessage.From.First() as MailboxAddress)?.Address;
                var fromName = mimeMessage.From.First().Name ?? "SkillBridge Tutors";

                if (string.IsNullOrEmpty(toAddress) || string.IsNullOrEmpty(fromAddress))
                {
                    throw new InvalidOperationException("Invalid email addresses in MimeMessage");
                }

                // Get HTML body from the message
                var htmlBody = ExtractHtmlBody(mimeMessage);

                if (string.IsNullOrEmpty(htmlBody))
                {
                    _logger.LogWarning("Email message has no HTML body. Email to {Email} will be sent with empty body", toAddress);
                }

                // Prepare Resend request
                var request = new ResendEmailRequest
                {
                    From = $"{fromName} <{fromAddress}>",
                    To = toAddress,
                    Subject = subject,
                    Html = htmlBody
                };

                // Send via Resend API
                using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
                // Resend API expects: Authorization: Bearer re_xxxxx (without quotes around the scheme)
                httpRequestMessage.Headers.Add("Authorization", $"Bearer {resendApiKey}");
                httpRequestMessage.Content = JsonContent.Create(request);

                var response = await _httpClient.SendAsync(httpRequestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Resend API error: StatusCode={StatusCode}, Response={Response}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"Resend API returned {response.StatusCode}: {responseContent}");
                }

                // Parse response to get message ID
                var messageId = ParseResendResponse(responseContent);

                if (string.IsNullOrEmpty(messageId))
                {
                    _logger.LogWarning("Resend API returned success but no message ID in response: {Response}", responseContent);
                }

                _logger.LogInformation("Email sent via Resend to {Email} with MessageId={MessageId}", toAddress, messageId);
                return messageId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Resend: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Extracts the HTML body from a MimeMessage, supporting both simple and multipart messages
        /// </summary>
        private string ExtractHtmlBody(MimeMessage mimeMessage)
        {
            if (mimeMessage.Body is TextPart textPart)
            {
                // Simple text part - return if it's HTML
                if (textPart.IsHtml)
                {
                    return textPart.Text;
                }
                // If it's plain text, return it anyway (Resend can handle it)
                return textPart.Text;
            }

            if (mimeMessage.Body is Multipart multipart)
            {
                // Prefer HTML part over plain text
                var htmlPart = multipart.FirstOrDefault(x => x is TextPart tp && tp.IsHtml);
                if (htmlPart is TextPart htmlTextPart)
                {
                    return htmlTextPart.Text;
                }

                // Fallback to first text part
                var textPart2 = multipart.FirstOrDefault(x => x is TextPart);
                if (textPart2 is TextPart fallbackTextPart)
                {
                    return fallbackTextPart.Text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Parses the Resend API response to extract the message ID
        /// </summary>
        private string ParseResendResponse(string responseContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("id", out var idElement))
                {
                    return idElement.GetString() ?? string.Empty;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse Resend API response as JSON");
            }

            return string.Empty;
        }

        // DTOs for Resend API
        private class ResendEmailRequest
        {
            [JsonPropertyName("from")]
            public string From { get; set; } = string.Empty;

            [JsonPropertyName("to")]
            public string To { get; set; } = string.Empty;

            [JsonPropertyName("subject")]
            public string Subject { get; set; } = string.Empty;

            [JsonPropertyName("html")]
            public string Html { get; set; } = string.Empty;
        }

        private class ResendEmailResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;
        }
    }
}
