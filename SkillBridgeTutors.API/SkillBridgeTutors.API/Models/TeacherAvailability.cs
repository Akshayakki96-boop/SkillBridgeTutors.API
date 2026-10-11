using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class TeacherAvailability
    {
        [Key]
        [Column("AvailabilityId")]
        public long AvailabilityId { get; set; }

        [Column("TeacherId")]
        public long TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public Teacher? Teacher { get; set; }

        [Column("DayOfWeek")]
        public DayOfWeek DayOfWeek { get; set; }

        [Column("StartTime")]
        public TimeOnly StartTime { get; set; }

        [Column("EndTime")]
        public TimeOnly EndTime { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
