using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    /// <summary>
    /// Base DTO for Google OAuth token received from frontend
    /// </summary>
    public class GoogleSignInDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;

        [Required]
        public string AccessToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for student Google sign-in with optional student details
    /// </summary>
    public class StudentGoogleSignInDto : GoogleSignInDto
    {
        public string? Phone { get; set; }
        public string? ParentFirstName { get; set; }
        public string? ParentLastName { get; set; }
        public string? ClassYear { get; set; }
        public string? Subject { get; set; }
        public string? Address { get; set; }
    }

    /// <summary>
    /// DTO for teacher Google sign-in with optional teacher details
    /// </summary>
    public class TeacherGoogleSignInDto : GoogleSignInDto
    {
        public string? Subject { get; set; }
        public string? Bio { get; set; }
    }
}
