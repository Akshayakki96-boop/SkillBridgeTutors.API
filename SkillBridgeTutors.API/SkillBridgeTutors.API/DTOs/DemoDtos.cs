using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SkillBridgeTutors.API.DTOs
{
    public class DemoSlotDto
    {
        public long SlotId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class BookDemoDto
    {
        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("subject")]
        public string Subject { get; set; } = string.Empty;

        [JsonPropertyName("slotId")]
        public long SlotId { get; set; }
    }

    public class RescheduleDemoDto
    {
        [Required]
        public long BookingId { get; set; }

        [Required]
        public long NewSlotId { get; set; }

        public string? Reason { get; set; }
    }

    public class CancelDemoDto
    {
        [Required]
        public long BookingId { get; set; }

        public string? Reason { get; set; }
    }

    public class DemoBookingResponseDto
    {
        public long BookingId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? MeetingLink { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookedAt { get; set; }
    }
}
