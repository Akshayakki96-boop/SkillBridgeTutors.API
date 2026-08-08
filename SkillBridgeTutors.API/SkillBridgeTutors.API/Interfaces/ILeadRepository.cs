using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface ILeadRepository
    {
        Task<Lead> CreateAsync(Lead lead);
        Task<Lead?> GetByIdAsync(int id);
        Task<IEnumerable<Lead>> GetAllAsync();
        Task UpdateAsync(Lead lead);
    }
}
