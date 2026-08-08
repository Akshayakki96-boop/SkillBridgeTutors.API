namespace SkillBridgeTutors.API.Models
{
    public class Lead
    {
        public int Id { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string CallStatus { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CallRecord> CallRecords { get; set; } = new List<CallRecord>();
        public ICollection<DemoBooking> DemoBookings { get; set; } = new List<DemoBooking>();
    }
}
