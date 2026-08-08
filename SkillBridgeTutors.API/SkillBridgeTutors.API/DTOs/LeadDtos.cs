using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    public class CreateLeadDto
    {
        [Required]
        public string ParentName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Query { get; set; } = string.Empty;
    }

    public class LeadResponseDto
    {
        public int Id { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string CallStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
