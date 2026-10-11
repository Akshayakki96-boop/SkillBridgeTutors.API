namespace SkillBridgeTutors.API.DTOs
{
    public class CreateTeacherAvailabilityDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class TeacherAvailabilityDto
    {
        public long AvailabilityId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class ScheduleSuggestionRequestDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Subject { get; set; }
    }

    public class ScheduleSuggestionDto
    {
        public long TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string Subjects { get; set; } = string.Empty;
        public long AvailabilityId { get; set; }
        public TimeOnly AvailableFrom { get; set; }
        public TimeOnly AvailableTo { get; set; }
    }
}
