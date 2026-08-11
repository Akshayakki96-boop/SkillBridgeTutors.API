using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/leads")]
    public class LeadsController : ControllerBase
    {
        private readonly ILeadRepository _leadRepository;
        private readonly IRetellService _retellService;
        private readonly ICallRecordRepository _callRecordRepository;
        private readonly ILogger<LeadsController> _logger;

        public LeadsController(
            ILeadRepository leadRepository,
            IRetellService retellService,
            ICallRecordRepository callRecordRepository,
            ILogger<LeadsController> logger)
        {
            _leadRepository = leadRepository;
            _retellService = retellService;
            _callRecordRepository = callRecordRepository;
            _logger = logger;
        }

        /// <summary>
        /// Submit a parent enquiry and trigger an outbound Retell AI call.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateLead([FromBody] CreateLeadDto dto)
        {
            var lead = new Lead
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Subject = dto.Subject,
                Query = dto.Query,
                Status = "New",
                Source = "Website"
            };

            await _leadRepository.CreateAsync(lead);

            // Trigger Retell AI outbound call
            try
            {
                var callId = await _retellService.TriggerOutboundCallAsync(lead);

                await _callRecordRepository.CreateAsync(new CallRecord
                {
                    LeadId = lead.LeadId,
                    RetellCallId = callId,
                    PhoneNumber = lead.Phone,
                    CallDirection = "outbound",
                    CallStatus = "initiated"
                });

                lead.Status = "Calling";
                await _leadRepository.UpdateAsync(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to trigger Retell call for lead {LeadId}", lead.LeadId);
                lead.Status = "CallPending";
                await _leadRepository.UpdateAsync(lead);
            }

            return CreatedAtAction(nameof(GetLead), new { id = lead.LeadId }, MapToDto(lead));
        }

        /// <summary>
        /// Get a single lead by ID.
        /// </summary>
        [Authorize]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetLead(long id)
        {
            var lead = await _leadRepository.GetByIdAsync(id);
            if (lead == null) return NotFound();
            return Ok(MapToDto(lead));
        }

        /// <summary>
        /// Get all leads (Admin Dashboard).
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllLeads()
        {
            var leads = await _leadRepository.GetAllAsync();
            return Ok(leads.Select(MapToDto));
        }

        private static LeadResponseDto MapToDto(Lead lead) => new()
        {
            LeadId = lead.LeadId,
            FullName = lead.FullName,
            Email = lead.Email,
            Phone = lead.Phone,
            Subject = lead.Subject,
            Query = lead.Query,
            Status = lead.Status,
            Source = lead.Source,
            CreatedAt = lead.CreatedAt
        };
    }
}
