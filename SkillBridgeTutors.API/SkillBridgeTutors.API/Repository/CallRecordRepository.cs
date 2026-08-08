using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Repository
{
    public class CallRecordRepository : ICallRecordRepository
    {
        private readonly ApplicationDbContext _context;

        public CallRecordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CallRecord> CreateAsync(CallRecord callRecord)
        {
            _context.CallRecords.Add(callRecord);
            await _context.SaveChangesAsync();
            return callRecord;
        }

        public async Task<CallRecord?> GetByRetellCallIdAsync(string retellCallId)
        {
            return await _context.CallRecords
                .FirstOrDefaultAsync(c => c.RetellCallId == retellCallId);
        }

        public async Task UpdateAsync(CallRecord callRecord)
        {
            _context.CallRecords.Update(callRecord);
            await _context.SaveChangesAsync();
        }
    }
}
