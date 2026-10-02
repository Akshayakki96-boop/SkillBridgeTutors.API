using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(AdminUser user);
        string GenerateToken(Student student);
        string GenerateToken(Teacher teacher);
        string GenerateStudentPasswordResetToken(Student student, int expiresInMinutes = 30);
        bool TryGetStudentIdFromPasswordResetToken(string token, out int studentId);
    }
}
