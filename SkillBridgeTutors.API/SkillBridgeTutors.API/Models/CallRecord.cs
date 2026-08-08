using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class CallRecord
    {
        [Key]
        [Column("CallRecordId")]
        public long CallRecordId { get; set; }

        [Column("LeadId")]
        public long LeadId { get; set; }

        [ForeignKey("LeadId")]
        public Lead Lead { get; set; } = null!;

        [Column("RetellCallId")]
        public string RetellCallId { get; set; } = string.Empty;

        [Column("PhoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("CallDirection")]
        public string CallDirection { get; set; } = "outbound";

        [Column("CallStatus")]
        public string CallStatus { get; set; } = "initiated";

        [Column("StartedAt")]
        public DateTime? StartedAt { get; set; }

        [Column("EndedAt")]
        public DateTime? EndedAt { get; set; }

        [Column("DurationSeconds")]
        public int? DurationSeconds { get; set; }

        [Column("RecordingUrl")]
        public string? RecordingUrl { get; set; }

        [Column("Summary")]
        public string? Summary { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
