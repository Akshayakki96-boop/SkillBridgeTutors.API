using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class RegularClass
    {
        [Key]
        [Column("ClassId")]
        public long ClassId { get; set; }

        [Column("StudentId")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Column("TeacherId")]
        public long TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher? Teacher { get; set; }

        [Column("Subject")]
        public string Subject { get; set; } = string.Empty;

        [Column("StartTime")]
        public DateTime StartTime { get; set; }

        [Column("EndTime")]
        public DateTime EndTime { get; set; }

        [Column("RecurrencePattern")]
        public string? RecurrencePattern { get; set; } // Weekly, BiWeekly, Monthly, OneTime

        [Column("Status")]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, Rescheduled

        [Column("ClassType")]
        public string ClassType { get; set; } = "Regular";

        [Column("MeetingLink")]
        public string? MeetingLink { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
