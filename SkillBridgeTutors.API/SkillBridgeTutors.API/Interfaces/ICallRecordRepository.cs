using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface ICallRecordRepository
    {
        Task<CallRecord> CreateAsync(CallRecord callRecord);
        Task<CallRecord?> GetByRetellCallIdAsync(string retellCallId);
        Task UpdateAsync(CallRecord callRecord);
    }
}
