using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Interfaces
{
    public interface IDemoRepository
    {
        Task<IEnumerable<DemoSlot>> GetAvailableSlotsAsync(int count = 5);
        Task<IEnumerable<DemoSlot>> GetSlotsAsync(DateTime? fromUtc = null, DateTime? toUtc = null);
        Task<DemoSlot> CreateSlotAsync(DemoSlot slot);
        Task<DemoSlot?> GetSlotByIdAsync(long slotId);
        Task<DemoBooking> CreateBookingAsync(DemoBooking booking);
        Task<DemoBooking?> GetBookingByIdAsync(long bookingId);
        Task<IEnumerable<DemoBooking>> GetBookingsAsync();
        Task UpdateBookingAsync(DemoBooking booking);
        Task UpdateSlotAsync(DemoSlot slot);
        Task<Teacher?> GetAvailableTeacherAsync(long slotId, string subject);
    }
}
