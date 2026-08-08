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
                Id = s.Id,
                SlotDateTime = s.SlotDateTime,
                TutorName = s.TutorName
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
            if (slot.IsBooked) return Conflict(new { message = "Slot is already booked. Please choose another slot." });

            // Find lead by email and phone
            var leads = await _leadRepository.GetAllAsync();
            var lead = leads.FirstOrDefault(l => l.Email == dto.Email && l.Phone == dto.Phone);
            if (lead == null) return NotFound(new { message = "Lead not found. Please submit an enquiry first." });

            slot.IsBooked = true;
            await _demoRepository.UpdateSlotAsync(slot);

            var booking = new DemoBooking
            {
                LeadId = lead.Id,
                DemoSlotId = slot.Id,
                StudentName = dto.StudentName,
                Grade = dto.Grade,
                Curriculum = dto.Curriculum,
                Subject = dto.Subject,
                Status = "Booked"
            };

            await _demoRepository.CreateBookingAsync(booking);

            // Reload with slot for email
            var fullBooking = await _demoRepository.GetBookingByIdAsync(booking.Id);

            try
            {
                await _emailService.SendDemoConfirmationAsync(lead, fullBooking!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Confirmation email failed for booking {BookingId}", booking.Id);
            }

            return Ok(new DemoBookingResponseDto
            {
                Id = fullBooking!.Id,
                StudentName = fullBooking.StudentName,
                Subject = fullBooking.Subject,
                Grade = fullBooking.Grade,
                Curriculum = fullBooking.Curriculum,
                SlotDateTime = fullBooking.DemoSlot.SlotDateTime,
                TutorName = fullBooking.DemoSlot.TutorName,
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
            if (newSlot.IsBooked) return Conflict(new { message = "New slot is already booked." });

            // Free the old slot
            var oldSlot = await _demoRepository.GetSlotByIdAsync(booking.DemoSlotId);
            if (oldSlot != null)
            {
                oldSlot.IsBooked = false;
                await _demoRepository.UpdateSlotAsync(oldSlot);
            }

            newSlot.IsBooked = true;
            await _demoRepository.UpdateSlotAsync(newSlot);

            booking.DemoSlotId = newSlot.Id;
            booking.Status = "Rescheduled";
            await _demoRepository.UpdateBookingAsync(booking);

            return Ok(new { message = "Booking rescheduled successfully.", newSlotDateTime = newSlot.SlotDateTime });
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

            var slot = await _demoRepository.GetSlotByIdAsync(booking.DemoSlotId);
            if (slot != null)
            {
                slot.IsBooked = false;
                await _demoRepository.UpdateSlotAsync(slot);
            }

            booking.Status = "Cancelled";
            await _demoRepository.UpdateBookingAsync(booking);

            return Ok(new { message = "Booking cancelled successfully." });
        }
    }
}
