using Microsoft.AspNetCore.Mvc;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/demo")]
    public class DemoController : ControllerBase
    {
        private readonly IDemoRepository _demoRepository;
        private readonly ILeadRepository _leadRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<DemoController> _logger;

        public DemoController(
            IDemoRepository demoRepository,
            ILeadRepository leadRepository,
            IEmailService emailService,
            ILogger<DemoController> logger)
        {
            _demoRepository = demoRepository;
            _leadRepository = leadRepository;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the next 5 available demo slots. Called by Retell AI agent.
        /// </summary>
        [HttpGet("slots")]
        public async Task<IActionResult> GetAvailableSlots()
        {
            var slots = await _demoRepository.GetAvailableSlotsAsync(5);
            var result = slots.Select(s => new DemoSlotDto
            {
                SlotId = s.SlotId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                DayName = s.StartTime.ToString("dddd"),
                FormattedSlot = s.StartTime.ToString("dddd, dd MMMM yyyy 'from' HH:mm 'to' ") + s.EndTime.ToString("HH:mm 'UTC'")
            });
            return Ok(result);
        }

        /// <summary>
        /// Books a demo slot. Called by Retell AI agent after parent selects a slot.
        /// </summary>
        [HttpPost("book")]
        public async Task<IActionResult> BookDemo([FromBody] BookDemoDto dto)
        {
            var slot = await _demoRepository.GetSlotByIdAsync(dto.SlotId);
            if (slot == null) return NotFound(new { message = "Slot not found." });
            if (!slot.IsAvailable) return Conflict(new { message = "Slot is already booked. Please choose another slot." });

            // Find lead by email or phone
            var leads = await _leadRepository.GetAllAsync();
            var lead = leads.FirstOrDefault(l => l.Email == dto.Email)
                     ?? leads.FirstOrDefault(l => l.Phone == dto.Phone)
                     ?? leads.FirstOrDefault(l => l.FullName == dto.CustomerName);
            if (lead == null) return NotFound(new { message = "Lead not found. Please submit an enquiry first." });

            slot.IsAvailable = false;
            await _demoRepository.UpdateSlotAsync(slot);

            var booking = new DemoBooking
            {
                LeadId = lead.LeadId,
                SlotId = slot.SlotId,
                Status = "Booked"
            };

            await _demoRepository.CreateBookingAsync(booking);

            var fullBooking = await _demoRepository.GetBookingByIdAsync(booking.BookingId);

            try
            {
                await _emailService.SendDemoConfirmationAsync(lead, fullBooking!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Confirmation email failed for booking {BookingId}", booking.BookingId);
            }

            return Ok(new DemoBookingResponseDto
            {
                BookingId = fullBooking!.BookingId,
                Subject = lead.Subject,
                StartTime = fullBooking.DemoSlot.StartTime,
                EndTime = fullBooking.DemoSlot.EndTime,
                MeetingLink = fullBooking.MeetingLink,
                Status = fullBooking.Status,
                BookedAt = fullBooking.BookedAt
            });
        }

        /// <summary>
        /// Reschedules an existing booking. Called by Retell AI agent.
        /// </summary>
        [HttpPost("reschedule")]
        public async Task<IActionResult> RescheduleDemo([FromBody] RescheduleDemoDto dto)
        {
            var booking = await _demoRepository.GetBookingByIdAsync(dto.BookingId);
            if (booking == null) return NotFound(new { message = "Booking not found." });
            if (booking.Status == "Cancelled") return BadRequest(new { message = "Cannot reschedule a cancelled booking." });

            var newSlot = await _demoRepository.GetSlotByIdAsync(dto.NewSlotId);
            if (newSlot == null) return NotFound(new { message = "New slot not found." });
            if (!newSlot.IsAvailable) return Conflict(new { message = "New slot is already booked." });

            // Free the old slot
            var oldSlot = await _demoRepository.GetSlotByIdAsync(booking.SlotId);
            if (oldSlot != null)
            {
                oldSlot.IsAvailable = true;
                await _demoRepository.UpdateSlotAsync(oldSlot);
            }

            newSlot.IsAvailable = false;
            await _demoRepository.UpdateSlotAsync(newSlot);

            booking.RescheduledFromBookingId = booking.BookingId;
            booking.SlotId = newSlot.SlotId;
            booking.Status = "Rescheduled";
            await _demoRepository.UpdateBookingAsync(booking);

            return Ok(new { message = "Booking rescheduled successfully.", newStartTime = newSlot.StartTime, newEndTime = newSlot.EndTime });
        }

        /// <summary>
        /// Cancels a booking. Called by Retell AI agent.
        /// </summary>
        [HttpPost("cancel")]
        public async Task<IActionResult> CancelDemo([FromBody] CancelDemoDto dto)
        {
            var booking = await _demoRepository.GetBookingByIdAsync(dto.BookingId);
            if (booking == null) return NotFound(new { message = "Booking not found." });
            if (booking.Status == "Cancelled") return BadRequest(new { message = "Booking is already cancelled." });

            var slot = await _demoRepository.GetSlotByIdAsync(booking.SlotId);
            if (slot != null)
            {
                slot.IsAvailable = true;
                await _demoRepository.UpdateSlotAsync(slot);
            }

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = dto.Reason;
            await _demoRepository.UpdateBookingAsync(booking);

            return Ok(new { message = "Booking cancelled successfully." });
        }
    }
}
