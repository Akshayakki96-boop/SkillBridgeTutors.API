using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IRegularClassRepository
    {
        Task<RegularClass> CreateAsync(RegularClass regularClass);
        Task<RegularClass?> GetByIdAsync(long classId);
        Task<IEnumerable<RegularClass>> GetByTeacherAsync(long teacherId);
        Task<IEnumerable<RegularClass>> GetByStudentAsync(int studentId);
        Task<IEnumerable<RegularClass>> GetUpcomingAsync(int days = 30);
        Task UpdateAsync(RegularClass regularClass);
        Task DeleteAsync(RegularClass regularClass);
    }
}
