using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class EmailLog
    {
        [Key]
        [Column("EmailLogId")]
        public long EmailLogId { get; set; }

        [Column("LeadId")]
        public long? LeadId { get; set; }

        [Column("BookingId")]
        public long? BookingId { get; set; }

        /// <summary>Recipient email address</summary>
        [Column("ToEmail")]
        [MaxLength(256)]
        public string ToEmail { get; set; } = string.Empty;

        [Column("Subject")]
        [MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>ParentConfirmation or TeacherNotification</summary>
        [Column("EmailType")]
        [MaxLength(100)]
        public string EmailType { get; set; } = string.Empty;

        /// <summary>Sent | Failed</summary>
        [Column("Status")]
        [MaxLength(20)]
        public string Status { get; set; } = "Sent";

        [Column("ProviderMessageId")]
        [MaxLength(256)]
        public string? ProviderMessageId { get; set; }

        [Column("ErrorMessage")]
        public string? ErrorMessage { get; set; }

        [Column("SentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
