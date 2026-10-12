using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IStudentRepository
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student?> GetByIdAsync(int studentId);
        Task<Student?> GetByEmailAsync(string email);
        Task<Student> CreateAsync(Student student);
        Task UpdateAsync(Student student);
        Task<bool> ExistsByEmailAsync(string email);
    }
}
