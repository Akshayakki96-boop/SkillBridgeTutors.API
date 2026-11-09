using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    /// <summary>
    /// Base DTO for Google OAuth token received from frontend
    /// </summary>
    public class GoogleSignInDto
    {
        /// <summary>
        /// Google ID Token (required) - used for identity verification
        /// Obtained from @react-oauth/google library
        /// </summary>
        [Required(ErrorMessage = "IdToken is required for authentication")]
        public string IdToken { get; set; } = string.Empty;

        /// <summary>
        /// Google Access Token (optional) - used for accessing user data
        /// Not provided by @react-oauth/google library in ID token flow
        /// </summary>
        public string? AccessToken { get; set; }

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

    /// <summary>
    /// DTO for updating student profile (after sign-in or during profile edit)
    /// </summary>
    public class UpdateStudentProfileDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? ParentFirstName { get; set; }
        public string? ParentLastName { get; set; }
        public string? ClassYear { get; set; }
        public string? Subject { get; set; }
        public string? Address { get; set; }
    }
}
