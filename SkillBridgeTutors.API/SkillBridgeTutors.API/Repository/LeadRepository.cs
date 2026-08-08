using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Repository
{
    public class LeadRepository : ILeadRepository
    {
        private readonly ApplicationDbContext _context;

        public LeadRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Lead> CreateAsync(Lead lead)
        {
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
            return lead;
        }

        public async Task<Lead?> GetByIdAsync(int id)
        {
            return await _context.Leads
                .Include(l => l.DemoBookings).ThenInclude(b => b.DemoSlot)
                .Include(l => l.CallRecords)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Lead>> GetAllAsync()
        {
            return await _context.Leads
                .Include(l => l.DemoBookings).ThenInclude(b => b.DemoSlot)
                .Include(l => l.CallRecords)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Lead lead)
        {
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync();
        }
    }
}
