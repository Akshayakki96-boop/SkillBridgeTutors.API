using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    public class CreateRegularClassDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public long TeacherId { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        public string? RecurrencePattern { get; set; } // Weekly, BiWeekly, Monthly, OneTime

        public string? MeetingLink { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// Composes a DateTime from StartDate and StartTime.
        /// </summary>
        public DateTime GetStartDateTime() => StartDate.ToDateTime(StartTime);

        /// <summary>
        /// Composes a DateTime from StartDate and EndTime.
        /// </summary>
        public DateTime GetEndDateTime() => StartDate.ToDateTime(EndTime);
    }

    public class UpdateRegularClassDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? RecurrencePattern { get; set; }
        public string? MeetingLink { get; set; }
        public string? Notes { get; set; }
    }

    public class RegularClassResponseDto
    {
        public long ClassId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public long TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? RecurrencePattern { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ClassType { get; set; } = "Regular";
        public string? MeetingLink { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RegularClassDto
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
