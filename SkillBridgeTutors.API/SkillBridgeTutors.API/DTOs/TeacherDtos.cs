namespace SkillBridgeTutors.API.DTOs
{
    public class CreateTeacherDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty; // Comma-separated e.g. "Math,Science"
        public string? Password { get; set; } // Optional - admin can set during creation
    }

    public class UpdateTeacherDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class TeacherResponseDto
    {
        public long TeacherId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TeacherLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class TeacherAuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public long TeacherId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class TeacherApplicationDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty; // Comma-separated
        public string? Message { get; set; } // Optional message from applicant
        public string? QualificationDetails { get; set; } // Optional qualifications
    }
}
