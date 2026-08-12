using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    public class Teacher
    {
        [Key]
        [Column("TeacherId")]
        public long TeacherId { get; set; }

        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("Subjects")]
        public string Subjects { get; set; } = string.Empty; // Comma-separated e.g. "Math,Science"

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<DemoBooking> DemoBookings { get; set; } = new List<DemoBooking>();
    }
}
