using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    public class TeacherDashboardDto
    {
        public long TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Subjects { get; set; } = string.Empty;

        public int ClassesToday { get; set; }
        public int TotalStudents { get; set; }
        public decimal EarningsThisMonth { get; set; }
        public decimal AverageRating { get; set; }

        public List<ScheduledClassDto> TodaysSchedule { get; set; } = new();
        public List<UpcomingClassDto> UpcomingClasses { get; set; } = new();
        public List<StudentDto> AllStudents { get; set; } = new();
    }

    public class ScheduledClassDto
    {
        public long? BookingId { get; set; } // For demo classes
        public long? ClassId { get; set; } // For regular classes
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string? StudentPhone { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string ClassType { get; set; } = "Demo"; // Demo or Regular
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? MeetingLink { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UpcomingClassDto
    {
        public long? BookingId { get; set; } // For demo classes
        public long? ClassId { get; set; } // For regular classes
        public string StudentName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string ClassType { get; set; } = "Demo";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class StudentDto
    {
        public long? LeadId { get; set; } // For demo students
        public int? StudentId { get; set; } // For regular class students
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? ClassYear { get; set; }
        public int TotalClasses { get; set; }
        public DateTime FirstBookedAt { get; set; }
    }

    public class TeacherEarningsDto
    {
        public decimal ThisWeek { get; set; }
        public decimal ThisMonth { get; set; }
        public decimal Total { get; set; }
        public decimal ChangePercentage { get; set; }
    }
}
