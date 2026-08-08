using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface ILeadRepository
    {
        Task<Lead> CreateAsync(Lead lead);
        Task<Lead?> GetByIdAsync(long id);
        Task<IEnumerable<Lead>> GetAllAsync();
        Task UpdateAsync(Lead lead);
    }
}
