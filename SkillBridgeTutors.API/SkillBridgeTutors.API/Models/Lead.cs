using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class Lead
    {
        [Key]
        [Column("LeadId")]
        public long LeadId { get; set; }

        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("Phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("Subject")]
        public string Subject { get; set; } = string.Empty;

        [Column("Query")]
        public string Query { get; set; } = string.Empty;

        [Column("Status")]
        public string Status { get; set; } = "New";

        [Column("Source")]
        public string Source { get; set; } = "Website";

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CallRecord> CallRecords { get; set; } = new List<CallRecord>();
        public ICollection<DemoBooking> DemoBookings { get; set; } = new List<DemoBooking>();
    }
}
