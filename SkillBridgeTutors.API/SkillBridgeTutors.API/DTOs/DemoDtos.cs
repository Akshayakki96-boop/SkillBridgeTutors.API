using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
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
