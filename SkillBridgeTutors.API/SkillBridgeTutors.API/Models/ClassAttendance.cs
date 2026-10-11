using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class ClassAttendance
    {
        [Key]
        [Column("AttendanceId")]
        public long AttendanceId { get; set; }

        [Column("ClassId")]
        public long ClassId { get; set; }

        [ForeignKey("ClassId")]
        public RegularClass? RegularClass { get; set; }

        [Column("StudentId")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Column("AttendanceStatus")]
        public string AttendanceStatus { get; set; } = "Present"; // Present, Absent, Late

        [Column("AttendedAt")]
        public DateTime? AttendedAt { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
