using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class EmailLog
    {
        [Key]
        [Column("EmailLogId")]
        public long EmailLogId { get; set; }

        /// <summary>Parent confirmation or TeacherNotification</summary>
        [Column("EmailType")]
        [MaxLength(100)]
        public string EmailType { get; set; } = string.Empty;

        [Column("ToAddress")]
        [MaxLength(256)]
        public string ToAddress { get; set; } = string.Empty;

        [Column("Subject")]
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>BookingId the email relates to (nullable for non-booking emails)</summary>
        [Column("BookingId")]
        public long? BookingId { get; set; }

        /// <summary>Sent | Failed</summary>
        [Column("Status")]
        [MaxLength(20)]
        public string Status { get; set; } = "Sent";

        [Column("ErrorMessage")]
        public string? ErrorMessage { get; set; }

        [Column("SentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
