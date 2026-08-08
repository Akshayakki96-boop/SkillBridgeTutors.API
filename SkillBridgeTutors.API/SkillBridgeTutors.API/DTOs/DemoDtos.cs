using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    public class DemoSlotDto
    {
        public int Id { get; set; }
        public DateTime SlotDateTime { get; set; }
        public string? TutorName { get; set; }
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
        public int SlotId { get; set; }

        [Required]
        public string StudentName { get; set; } = string.Empty;

        [Required]
        public string Grade { get; set; } = string.Empty;

        [Required]
        public string Curriculum { get; set; } = string.Empty;
    }

    public class RescheduleDemoDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int NewSlotId { get; set; }
    }

    public class CancelDemoDto
    {
        [Required]
        public int BookingId { get; set; }
    }

    public class DemoBookingResponseDto
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string Curriculum { get; set; } = string.Empty;
        public DateTime SlotDateTime { get; set; }
        public string? TutorName { get; set; }
        public string? MeetingLink { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookedAt { get; set; }
    }
}
