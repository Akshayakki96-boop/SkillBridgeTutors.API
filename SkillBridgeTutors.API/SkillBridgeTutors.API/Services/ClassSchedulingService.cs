using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Services
{
    public class ClassSchedulingService : IClassSchedulingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITeacherAvailabilityRepository _availabilityRepository;
        private readonly IRegularClassRepository _regularClassRepository;
        private readonly IGoogleCalendarService _meetingService;
        private readonly IEmailService _emailService;

        public ClassSchedulingService(
            ApplicationDbContext context,
            ITeacherAvailabilityRepository availabilityRepository,
            IRegularClassRepository regularClassRepository,
            IGoogleCalendarService meetingService,
            IEmailService emailService)
        {
            _context = context;
            _availabilityRepository = availabilityRepository;
            _regularClassRepository = regularClassRepository;
            _meetingService = meetingService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<ScheduleSuggestionDto>> GetSuggestionsAsync(ScheduleSuggestionRequestDto request)
        {
            var suggestions = new List<ScheduleSuggestionDto>();
            if (request.EndTime <= request.StartTime)
                return suggestions;

            var slots = (await _availabilityRepository.GetActiveAsync())
                .Where(a => a.DayOfWeek == request.DayOfWeek
                            && a.StartTime <= request.StartTime
                            && a.EndTime >= request.EndTime)
                .ToList();

            var teacherIds = slots.Select(a => a.TeacherId).Distinct().ToList();
            var teachers = await _context.Teachers
                .Where(t => teacherIds.Contains(t.TeacherId) && t.IsActive)
                .ToListAsync();

            foreach (var teacher in teachers)
            {
                if (!string.IsNullOrWhiteSpace(request.Subject) && !HasSubject(teacher, request.Subject))
                    continue;

                var teacherClasses = await _regularClassRepository.GetByTeacherAsync(teacher.TeacherId);
                if (teacherClasses.Any(c => Overlaps(request.DayOfWeek, request.StartTime, request.EndTime, c)))
                    continue;

                var slot = slots.First(a => a.TeacherId == teacher.TeacherId);
                suggestions.Add(new ScheduleSuggestionDto
                {
                    TeacherId = teacher.TeacherId,
                    TeacherName = teacher.FullName,
                    Subjects = teacher.Subjects,
                    AvailabilityId = slot.AvailabilityId,
                    AvailableFrom = slot.StartTime,
                    AvailableTo = slot.EndTime
                });
            }

            return suggestions;
        }

        public async Task<(RegularClass? Class, string? Error)> CreateRegularClassAsync(CreateRegularClassDto dto)
        {
            if (dto.EndTime <= dto.StartTime)
                return (null, "End time must be after start time.");

            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null)
                return (null, "Student not found.");

            var teacher = await _context.Teachers.FindAsync(dto.TeacherId);
            if (teacher == null || !teacher.IsActive)
                return (null, "Teacher not found or inactive.");

            var day = dto.StartDate.DayOfWeek;
            var start = dto.StartTime;
            var end = dto.EndTime;
            var startDateTime = dto.GetStartDateTime();
            var endDateTime = dto.GetEndDateTime();

            var teacherSlots = await _availabilityRepository.GetByTeacherAsync(teacher.TeacherId);
            if (!teacherSlots.Any(a => a.DayOfWeek == day && a.StartTime <= start && a.EndTime >= end))
                return (null, "Teacher is not available at the requested time.");

            var teacherClasses = await _regularClassRepository.GetByTeacherAsync(teacher.TeacherId);
            if (teacherClasses.Any(c => Overlaps(day, start, end, c)))
                return (null, "Teacher already has a class at this time.");

            var studentClasses = await _regularClassRepository.GetByStudentAsync(student.Id);
            var studentClash = studentClasses.FirstOrDefault(c => Overlaps(day, start, end, c));
            if (studentClash != null)
                return (null, BuildStudentClashMessage(studentClash));

            var regularClass = new RegularClass
            {
                StudentId = student.Id,
                TeacherId = teacher.TeacherId,
                Subject = dto.Subject,
                StartTime = startDateTime,
                EndTime = endDateTime,
                RecurrencePattern = dto.RecurrencePattern ?? "Weekly",
                MeetingLink = dto.MeetingLink ?? await _meetingService.CreateClassMeetingAsync(),
                Notes = dto.Notes
            };

            var created = await _regularClassRepository.CreateAsync(regularClass);
            var full = await _regularClassRepository.GetByIdAsync(created.ClassId) ?? created;
            await _emailService.SendRegularClassEmailAsync(full, false);
            return (full, null);
        }

        public async Task<(RegularClass? Class, string? Error)> RescheduleRegularClassAsync(long classId, RescheduleRegularClassDto dto)
        {
            var existing = await _regularClassRepository.GetByIdAsync(classId);
            if (existing == null)
                return (null, "Class not found.");

            if (dto.EndTime <= dto.StartTime)
                return (null, "End time must be after start time.");

            if (dto.EndTime.Date != dto.StartTime.Date)
                return (null, "Start and end time must be on the same day.");

            var day = dto.StartTime.DayOfWeek;
            var start = TimeOnly.FromDateTime(dto.StartTime);
            var end = TimeOnly.FromDateTime(dto.EndTime);

            var teacherSlots = await _availabilityRepository.GetByTeacherAsync(existing.TeacherId);
            if (!teacherSlots.Any(a => a.DayOfWeek == day && a.StartTime <= start && a.EndTime >= end))
                return (null, "Teacher is not available at the requested time.");

            var teacherClasses = await _regularClassRepository.GetByTeacherAsync(existing.TeacherId);
            if (teacherClasses.Any(c => c.ClassId != classId && Overlaps(day, start, end, c)))
                return (null, "Teacher already has a class at this time.");

            var studentClasses = await _regularClassRepository.GetByStudentAsync(existing.StudentId);
            var studentClash = studentClasses.FirstOrDefault(c => c.ClassId != classId && Overlaps(day, start, end, c));
            if (studentClash != null)
                return (null, BuildStudentClashMessage(studentClash));

            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.UpdatedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.Reason))
                existing.Notes = $"Rescheduled: {dto.Reason}";

            await _regularClassRepository.UpdateAsync(existing);
            await _emailService.SendRegularClassEmailAsync(existing, true);
            return (existing, null);
        }

        private static string BuildStudentClashMessage(RegularClass clash)
        {
            return $"Student already has a class at this time: {clash.Subject} with {clash.Teacher?.FullName ?? "a teacher"} " +
                   $"on {clash.StartTime:dddd} {clash.StartTime:HH:mm}-{clash.EndTime:HH:mm}.";
        }

        private static bool HasSubject(Teacher teacher, string subject)
        {
            return teacher.Subjects
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Any(s => s.Equals(subject, StringComparison.OrdinalIgnoreCase));
        }

        private static bool Overlaps(DayOfWeek day, TimeOnly start, TimeOnly end, RegularClass existing)
        {
            if (existing.StartTime.DayOfWeek != day)
                return false;

            var existingStart = TimeOnly.FromDateTime(existing.StartTime);
            var existingEnd = TimeOnly.FromDateTime(existing.EndTime);
            return start < existingEnd && existingStart < end;
        }
    }
}
