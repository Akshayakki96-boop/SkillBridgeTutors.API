using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface ITeacherAvailabilityRepository
    {
        Task<IEnumerable<TeacherAvailability>> GetActiveAsync();
        Task<IEnumerable<TeacherAvailability>> GetByTeacherAsync(long teacherId);
        Task<TeacherAvailability?> GetByIdAsync(long availabilityId);
        Task<TeacherAvailability> CreateAsync(TeacherAvailability availability);
        Task DeleteAsync(TeacherAvailability availability);
    }
}
