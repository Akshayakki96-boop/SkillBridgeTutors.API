namespace SkillBridgeTutors.API.DTOs
{
    public class RetellWebhookDto
    {
        public string Event { get; set; } = string.Empty;
        public RetellCallDto? Call { get; set; }
    }

    public class RetellCallDto
    {
        public string CallId { get; set; } = string.Empty;
        public string CallStatus { get; set; } = string.Empty;
        public string? Transcript { get; set; }
        public string? RecordingUrl { get; set; }
        public int? DurationMs { get; set; }
        public string? CallSummary { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
