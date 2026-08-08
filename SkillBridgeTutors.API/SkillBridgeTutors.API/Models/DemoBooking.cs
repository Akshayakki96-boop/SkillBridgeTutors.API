namespace SkillBridgeTutors.API.Models
{
    public class DemoBooking
    {
        public int Id { get; set; }
        public int LeadId { get; set; }
        public Lead Lead { get; set; } = null!;
        public int DemoSlotId { get; set; }
        public DemoSlot DemoSlot { get; set; } = null!;
        public string StudentName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Curriculum { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = "Booked"; // Booked, Rescheduled, Cancelled
        public string? MeetingLink { get; set; }
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;
    }
}
