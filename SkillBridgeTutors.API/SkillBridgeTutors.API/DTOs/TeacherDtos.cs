namespace SkillBridgeTutors.API.DTOs
{
    public class CreateTeacherDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty; // Comma-separated e.g. "Math,Science"
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
}
