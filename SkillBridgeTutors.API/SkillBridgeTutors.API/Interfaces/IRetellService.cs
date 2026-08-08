using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IRetellService
    {
        Task<string> TriggerOutboundCallAsync(Lead lead);
    }
}
