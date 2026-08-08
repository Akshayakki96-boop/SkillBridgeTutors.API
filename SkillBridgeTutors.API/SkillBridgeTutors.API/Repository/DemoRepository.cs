using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Repository
{
    public class DemoRepository : IDemoRepository
    {
        private readonly ApplicationDbContext _context;

        public DemoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DemoSlot>> GetAvailableSlotsAsync(int count = 5)
        {
            return await _context.DemoSlots
                .Where(s => !s.IsBooked && s.SlotDateTime > DateTime.UtcNow)
                .OrderBy(s => s.SlotDateTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<DemoSlot?> GetSlotByIdAsync(int slotId)
        {
            return await _context.DemoSlots.FindAsync(slotId);
        }

        public async Task<DemoBooking> CreateBookingAsync(DemoBooking booking)
        {
            _context.DemoBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<DemoBooking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.DemoBookings
                .Include(b => b.Lead)
                .Include(b => b.DemoSlot)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task UpdateBookingAsync(DemoBooking booking)
        {
            _context.DemoBookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSlotAsync(DemoSlot slot)
        {
            _context.DemoSlots.Update(slot);
            await _context.SaveChangesAsync();
        }
    }
}
