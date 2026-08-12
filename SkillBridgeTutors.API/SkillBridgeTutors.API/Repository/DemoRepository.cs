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
                .Where(s => s.IsAvailable && s.StartTime > DateTime.UtcNow)
                .OrderBy(s => s.StartTime)
                .Take(count)
                .ToListAsync();
        }

        public async Task<DemoSlot?> GetSlotByIdAsync(long slotId)
        {
            return await _context.DemoSlots.FindAsync(slotId);
        }

        public async Task<DemoBooking> CreateBookingAsync(DemoBooking booking)
        {
            _context.DemoBookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<DemoBooking?> GetBookingByIdAsync(long bookingId)
        {
            return await _context.DemoBookings
                .Include(b => b.Lead)
                .Include(b => b.DemoSlot)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task UpdateBookingAsync(DemoBooking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _context.DemoBookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSlotAsync(DemoSlot slot)
        {
            slot.UpdatedAt = DateTime.UtcNow;
            _context.DemoSlots.Update(slot);
            await _context.SaveChangesAsync();
        }

        public async Task<Teacher?> GetAvailableTeacherAsync(long slotId, string subject)
        {
            // Get teacher IDs already booked for this slot
            var busyTeacherIds = await _context.DemoBookings
                .Where(b => b.SlotId == slotId && b.TeacherId != null && b.Status != "Cancelled")
                .Select(b => b.TeacherId!.Value)
                .ToListAsync();

            // Find an active teacher who teaches this subject and is not busy at this slot
            return await _context.Teachers
                .Where(t => t.IsActive
                    && t.Subjects.Contains(subject)
                    && !busyTeacherIds.Contains(t.TeacherId))
                .FirstOrDefaultAsync();
        }
    }
}
