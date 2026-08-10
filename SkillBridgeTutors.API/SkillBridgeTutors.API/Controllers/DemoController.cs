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
        private readonly IGoogleCalendarService _calendarService;
        private readonly ILogger<DemoController> _logger;

        public DemoController(
            IDemoRepository demoRepository,
            ILeadRepository leadRepository,
            IEmailService emailService,
            IGoogleCalendarService calendarService,
            ILogger<DemoController> logger)
        {
            _demoRepository = demoRepository;
            _leadRepository = leadRepository;
            _emailService = emailService;
            _calendarService = calendarService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the next 5 available demo slots. Called by Retell AI agent.
        /// </summary>
        [HttpGet("slots")]
        public async Task<IActionResult> GetAvailableSlots()
        {
            var slots = (await _demoRepository.GetAvailableSlotsAsync(5)).ToList();
            var result = slots.Select((s, index) => new DemoSlotDto
            {
                OptionNumber = index + 1,
                SlotId = s.SlotId,
                DayName = s.StartTime.ToString("dddd"),
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                FormattedSlot = $"{s.StartTime:dddd, dd MMMM yyyy} from {s.StartTime:HH:mm} to {s.EndTime:HH:mm} UTC"
            });
            return Ok(result);
        }

        /// <summary>
        /// Books a demo slot. Called by Retell AI agent after parent selects a slot.
        /// </summary>
        [HttpPost("book")]
        public async Task<IActionResult> BookDemo([FromBody] BookDemoDto dto)
        {
            // Retell AI sends either slotId or optionNumber (1-5) from getAvailableDemoSlots response
            var availableSlots = (await _demoRepository.GetAvailableSlotsAsync(5)).ToList();

            DemoSlot? slot;
            if (dto.SlotId.HasValue && dto.SlotId > 0)
            {
                slot = availableSlots.FirstOrDefault(s => s.SlotId == dto.SlotId.Value);
                if (slot == null)
                    return NotFound(new { message = $"Slot {dto.SlotId} not found. Please call getAvailableDemoSlots first." });
            }
            else
            {
                var index = dto.OptionNumber - 1;
                if (index < 0 || index >= availableSlots.Count)
                    return NotFound(new { message = $"Option {dto.OptionNumber} not found. Please call getAvailableDemoSlots first." });
                slot = availableSlots[index];
            }

            if (!slot.IsAvailable)
                return Conflict(new { message = "Slot is already booked. Please choose another option." });

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

            // Generate Google Meet link
            try
            {
                var meetLink = await _calendarService.CreateMeetingAsync(lead, fullBooking!);
                fullBooking!.MeetingLink = meetLink;
                await _demoRepository.UpdateBookingAsync(fullBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Google Meet for booking {BookingId}", booking.BookingId);
            }

            // Send confirmation email
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
