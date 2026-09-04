namespace SkillBridgeTutors.API.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public string? ParentFirstName { get; set; }
        public string? ParentLastName { get; set; }
        public string? ClassYear { get; set; }
        public string? Subject { get; set; }
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
