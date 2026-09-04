using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(AdminUser user);
        string GenerateToken(Student student);
    }
}
