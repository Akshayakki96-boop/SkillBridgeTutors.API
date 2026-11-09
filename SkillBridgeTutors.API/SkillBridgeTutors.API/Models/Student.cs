using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillBridgeTutors.API.Models
{
    [Table("Students")]
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("FullName")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("PasswordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("Phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("ParentFirstName")]
        public string? ParentFirstName { get; set; }

        [Column("ParentLastName")]
        public string? ParentLastName { get; set; }

        [Column("ClassYear")]
        public string? ClassYear { get; set; }

        [Column("Subject")]
        public string? Subject { get; set; } = "Maths";

        [Column("Address")]
        public string? Address { get; set; }

        // OAuth provider tracking
        [Column("GoogleId")]
        public string? GoogleId { get; set; }

        [Column("AuthProvider")]
        public string? AuthProvider { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<RegularClass> RegularClasses { get; set; } = new List<RegularClass>();
    }
}
