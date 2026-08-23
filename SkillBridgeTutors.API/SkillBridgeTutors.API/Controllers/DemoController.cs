using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;
using Microsoft.AspNetCore.Authorization;

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
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DemoController(
            IDemoRepository demoRepository,
            ILeadRepository leadRepository,
            IEmailService emailService,
            IGoogleCalendarService calendarService,
            ILogger<DemoController> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _demoRepository = demoRepository;
            _leadRepository = leadRepository;
            _emailService = emailService;
            _calendarService = calendarService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
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

        [HttpGet("admin/slots")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSlots([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
        {
            var slots = await _demoRepository.GetSlotsAsync(fromUtc, toUtc);
            var result = slots.Select(s => new AdminDemoSlotDto
            {
                SlotId = s.SlotId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsAvailable = s.IsAvailable,
                CreatedAt = s.CreatedAt
            });
            return Ok(result);
        }

        [HttpPost("admin/slots")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSlot([FromBody] CreateDemoSlotDto dto)
        {
            if (dto.EndTime <= dto.StartTime)
                return BadRequest(new { message = "EndTime must be greater than StartTime." });

            if (dto.StartTime <= DateTime.UtcNow)
                return BadRequest(new { message = "Slot start time must be in the future (UTC)." });

            var existing = await _demoRepository.GetSlotsAsync(dto.StartTime.AddHours(-4), dto.EndTime.AddHours(4));
            var overlaps = existing.Any(s => dto.StartTime < s.EndTime && dto.EndTime > s.StartTime);
            if (overlaps)
                return Conflict(new { message = "Slot overlaps with an existing slot." });

            var slot = new DemoSlot
            {
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                IsAvailable = dto.IsAvailable
            };

            var created = await _demoRepository.CreateSlotAsync(slot);
            return Ok(new AdminDemoSlotDto
            {
                SlotId = created.SlotId,
                StartTime = created.StartTime,
                EndTime = created.EndTime,
                IsAvailable = created.IsAvailable,
                CreatedAt = created.CreatedAt
            });
        }

        [HttpPatch("admin/slots/{slotId:long}/availability")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSlotAvailability(long slotId, [FromBody] UpdateSlotAvailabilityDto dto)
        {
            var slot = await _demoRepository.GetSlotByIdAsync(slotId);
            if (slot == null)
                return NotFound(new { message = "Slot not found." });

            slot.IsAvailable = dto.IsAvailable;
            await _demoRepository.UpdateSlotAsync(slot);

            return Ok(new { message = "Slot availability updated.", slotId = slot.SlotId, isAvailable = slot.IsAvailable });
        }

        [HttpGet("admin/teachers/available")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAvailableTeacher([FromQuery] long slotId, [FromQuery] string subject)
        {
            if (slotId <= 0 || string.IsNullOrWhiteSpace(subject))
                return BadRequest(new { message = "slotId and subject are required." });

            var teacher = await _demoRepository.GetAvailableTeacherAsync(slotId, subject);
            if (teacher == null)
                return NotFound(new { message = "No teacher available for this slot and subject." });

            return Ok(new TeacherResponseDto
            {
                TeacherId = teacher.TeacherId,
                FullName = teacher.FullName,
                Email = teacher.Email,
                Subjects = teacher.Subjects,
                IsActive = teacher.IsActive,
                CreatedAt = teacher.CreatedAt
            });
        }

        [HttpGet("admin/bookings")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBookings([FromQuery] string? status)
        {
            var bookings = await _demoRepository.GetBookingsAsync();
            var filtered = string.IsNullOrWhiteSpace(status)
                ? bookings
                : bookings.Where(b => b.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            var result = filtered.Select(b => new AdminDemoBookingDto
            {
                BookingId = b.BookingId,
                LeadId = b.LeadId,
                StudentName = b.Lead.FullName,
                StudentEmail = b.Lead.Email,
                StudentPhone = b.Lead.Phone,
                Subject = b.Lead.Subject,
                Status = b.Status,
                StartTime = b.DemoSlot.StartTime,
                EndTime = b.DemoSlot.EndTime,
                TeacherName = b.Teacher?.FullName,
                MeetingLink = b.MeetingLink,
                BookedAt = b.BookedAt
            });

            return Ok(result);
        }

        /// <summary>
        /// Books a demo slot. Called by Retell AI agent after parent selects a slot.
        /// </summary>
        [HttpPost("book")]
        public async Task<IActionResult> BookDemo([FromBody] JsonElement rawBody)
        {
            // Log the exact raw payload Retell sends so we can diagnose field name mismatches
            _logger.LogInformation("BookDemo raw payload from Retell: {Payload}", rawBody.ToString());

            // Parse all known field name variants Retell might send
            var customerName = TryGetString(rawBody, "customerName", "customer_name", "name");
            var email        = TryGetString(rawBody, "email", "customerEmail", "customer_email");
            var phone        = TryGetString(rawBody, "phone", "customerPhone", "customer_phone", "phoneNumber", "phone_number");
            var subject      = TryGetString(rawBody, "subject");
            var slotId       = TryGetLong(rawBody,   "slotId", "slot_id", "SlotId");
            var optionNumber = TryGetInt(rawBody,    "optionNumber", "option_number", "option", "selectedOption", "selected_option");

            var dto = new BookDemoDto
            {
                CustomerName = customerName ?? string.Empty,
                Email        = email ?? string.Empty,
                Phone        = phone ?? string.Empty,
                Subject      = subject ?? string.Empty,
                SlotId       = slotId,
                OptionNumber = optionNumber ?? 0
            };

            _logger.LogInformation("BookDemo parsed — customerName={Name} email={Email} phone={Phone} slotId={SlotId} optionNumber={Option}",
                dto.CustomerName, dto.Email, dto.Phone, dto.SlotId, dto.OptionNumber);

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
                    return NotFound(new { message = $"Option {dto.OptionNumber} not found. Please call getAvailableDemoSlots first.", rawPayload = rawBody.ToString() });
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

            // Generate Jitsi Meet link (fast — no external API call)
            var meetLink = await _calendarService.CreateMeetingAsync(lead, fullBooking!);
            fullBooking!.MeetingLink = meetLink;
            await _demoRepository.UpdateBookingAsync(fullBooking);

            // Build response immediately — don't wait for emails or teacher assignment
            var response = new DemoBookingResponseDto
            {
                BookingId   = fullBooking.BookingId,
                Subject     = lead.Subject,
                StartTime   = fullBooking.DemoSlot.StartTime,
                EndTime     = fullBooking.DemoSlot.EndTime,
                MeetingLink = fullBooking.MeetingLink,
                Status      = fullBooking.Status,
                BookedAt    = fullBooking.BookedAt
            };

            // Capture only plain values — scoped services will be disposed when the request ends
            var bookingId   = fullBooking.BookingId;
            var slotIdVal   = slot.SlotId;
            var leadSubject = lead.Subject;

            // Run teacher assignment + emails in background (fire-and-forget)
            _ = Task.Run(async () =>
            {
                try
                {
                // Create a new DI scope so we get a fresh ApplicationDbContext
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var demoRepo     = scope.ServiceProvider.GetRequiredService<IDemoRepository>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var bgBooking = await demoRepo.GetBookingByIdAsync(bookingId);
                if (bgBooking == null)
                {
                    _logger.LogError("Background task: booking {BookingId} not found", bookingId);
                    return;
                }

                try
                {
                    var teacher = await demoRepo.GetAvailableTeacherAsync(slotIdVal, leadSubject);
                    if (teacher != null)
                    {
                        bgBooking.TeacherId = teacher.TeacherId;
                        await demoRepo.UpdateBookingAsync(bgBooking);
                        _logger.LogInformation("Teacher {TeacherId} assigned to booking {BookingId}", teacher.TeacherId, bookingId);

                        try 
                            { await emailService.SendTeacherNotificationAsync(teacher, bgBooking.Lead, bgBooking); }
                        catch (Exception ex) { _logger.LogError(ex, "Teacher email failed for booking {BookingId}", bookingId); }
                    }
                    else
                   {
                        _logger.LogWarning("No available teacher for slot {SlotId} subject {Subject}", slotIdVal, leadSubject);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background teacher assignment failed for booking {BookingId}", bookingId);
                }

                try { await emailService.SendDemoConfirmationAsync(bgBooking.Lead, bgBooking); }
                catch (Exception ex) { _logger.LogError(ex, "Parent confirmation email failed for booking {BookingId}", bookingId); }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "FATAL: Background task crashed for booking {BookingId}", bookingId);
                }
            });

            return Ok(response);
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

        // --- Helpers to safely extract values from raw JSON regardless of field name casing ---

        private static string? TryGetString(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
                if (el.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String)
                    return prop.GetString();
            return null;
        }

        private static long? TryGetLong(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
                if (el.TryGetProperty(key, out var prop) && prop.TryGetInt64(out var val))
                    return val;
            return null;
        }

        private static int? TryGetInt(JsonElement el, params string[] keys)
        {
            foreach (var key in keys)
                if (el.TryGetProperty(key, out var prop) && prop.TryGetInt32(out var val))
                    return val;
            return null;
        }
    }
}
