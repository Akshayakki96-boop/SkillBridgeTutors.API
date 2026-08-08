namespace SkillBridgeTutors.API.Models
{
    public class CallRecord
    {
        public int Id { get; set; }
        public int LeadId { get; set; }
        public Lead Lead { get; set; } = null!;
        public string RetellCallId { get; set; } = string.Empty;
        public string? Transcript { get; set; }
        public string? RecordingUrl { get; set; }
        public int? DurationSeconds { get; set; }
        public string? Summary { get; set; }
        public string CallStatus { get; set; } = "initiated"; // initiated, ongoing, ended, error
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
    }
}
