using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    public class StudentDashboardDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ClassYear { get; set; }
        public string? Subject { get; set; } = "Maths";
        
        public int TotalClasses { get; set; }
        public int CompletedClasses { get; set; }
        public int UpcomingClasses { get; set; }

        public List<StudentClassDto> TodaysClasses { get; set; } = new();
        public List<StudentClassDto> UpcomingClassesList { get; set; } = new();
        public List<StudentClassDto> PastClasses { get; set; } = new();
    }

    public class StudentClassDto
    {
        public long ClassId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? MeetingLink { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RecurrencePattern { get; set; }
    }
}
