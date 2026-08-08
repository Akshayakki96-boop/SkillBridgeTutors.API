using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class DemoBooking
    {
        [Key]
        [Column("BookingId")]
        public long BookingId { get; set; }

        [Column("LeadId")]
        public long LeadId { get; set; }

        [ForeignKey("LeadId")]
        public Lead Lead { get; set; } = null!;

        [Column("SlotId")]
        public long SlotId { get; set; }

        [ForeignKey("SlotId")]
        public DemoSlot DemoSlot { get; set; } = null!;

        [Column("Status")]
        public string Status { get; set; } = "Booked";

        [Column("BookedAt")]
        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        [Column("RescheduledFromBookingId")]
        public long? RescheduledFromBookingId { get; set; }

        [Column("CancelledAt")]
        public DateTime? CancelledAt { get; set; }

        [Column("CancellationReason")]
        public string? CancellationReason { get; set; }

        [Column("MeetingLink")]
        public string? MeetingLink { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
