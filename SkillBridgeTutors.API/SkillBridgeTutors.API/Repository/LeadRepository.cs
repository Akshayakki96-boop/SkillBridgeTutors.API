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

        public async Task<Lead?> GetByIdAsync(long id)
        {
            return await _context.Leads
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.LeadId == id);
        }

        public async Task<IEnumerable<Lead>> GetAllAsync()
        {
            return await _context.Leads
                .AsNoTracking()
                .OrderByDescending(l => l.CreatedAt)
                .Take(100)  // Limit to prevent loading too much data
                .ToListAsync();
        }

        public async Task UpdateAsync(Lead lead)
        {
            lead.UpdatedAt = DateTime.UtcNow;
            _context.Leads.Update(lead);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get lead with demo bookings (for demo details view)
        /// </summary>
        public async Task<Lead?> GetByIdWithBookingsAsync(long id)
        {
            return await _context.Leads
                .AsNoTracking()
                .Where(l => l.LeadId == id)
                .Include(l => l.DemoBookings)
                .ThenInclude(b => b.DemoSlot)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get lead with call records (for call history view)
        /// </summary>
        public async Task<Lead?> GetByIdWithCallsAsync(long id)
        {
            return await _context.Leads
                .AsNoTracking()
                .Where(l => l.LeadId == id)
                .Include(l => l.CallRecords)
                .FirstOrDefaultAsync();
        }
    }
}
