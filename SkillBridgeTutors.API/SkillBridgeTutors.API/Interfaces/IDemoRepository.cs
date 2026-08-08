using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IDemoRepository
    {
        Task<IEnumerable<DemoSlot>> GetAvailableSlotsAsync(int count = 5);
        Task<DemoSlot?> GetSlotByIdAsync(long slotId);
        Task<DemoBooking> CreateBookingAsync(DemoBooking booking);
        Task<DemoBooking?> GetBookingByIdAsync(long bookingId);
        Task UpdateBookingAsync(DemoBooking booking);
        Task UpdateSlotAsync(DemoSlot slot);
    }
}
