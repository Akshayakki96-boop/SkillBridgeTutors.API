using Microsoft.AspNetCore.Mvc;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/retell")]
    public class RetellWebhookController : ControllerBase
    {
        private readonly ICallRecordRepository _callRecordRepository;
        private readonly ILeadRepository _leadRepository;
        private readonly ILogger<RetellWebhookController> _logger;

        public RetellWebhookController(
            ICallRecordRepository callRecordRepository,
            ILeadRepository leadRepository,
            ILogger<RetellWebhookController> logger)
        {
            _callRecordRepository = callRecordRepository;
            _leadRepository = leadRepository;
            _logger = logger;
        }

        /// <summary>
        /// Receives call completion events from Retell AI.
        /// Stores transcript, recording URL, duration and summary.
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] RetellWebhookDto dto)
        {
            if (dto.Call == null)
            {
                _logger.LogWarning("Retell webhook received with no call data.");
                return BadRequest();
            }

            var callRecord = await _callRecordRepository.GetByRetellCallIdAsync(dto.Call.CallId);
            if (callRecord == null)
            {
                _logger.LogWarning("No call record found for Retell call ID {CallId}", dto.Call.CallId);
                return Ok(); // Acknowledge so Retell does not retry
            }

            callRecord.CallStatus = dto.Call.CallStatus;
            callRecord.RecordingUrl = dto.Call.RecordingUrl;
            callRecord.DurationSeconds = dto.Call.DurationMs.HasValue
                ? dto.Call.DurationMs.Value / 1000
                : null;
            callRecord.Summary = dto.Call.CallSummary;

            if (dto.Call.CallStatus is "ended" or "error")
            {
                callRecord.EndedAt = DateTime.UtcNow;

                var lead = await _leadRepository.GetByIdAsync(callRecord.LeadId);
                if (lead != null)
                {
                    lead.Status = dto.Call.CallStatus == "ended" ? "Completed" : "Failed";
                    await _leadRepository.UpdateAsync(lead);
                }
            }

            await _callRecordRepository.UpdateAsync(callRecord);

            _logger.LogInformation("Retell webhook processed for call {CallId}, status={Status}",
                dto.Call.CallId, dto.Call.CallStatus);

            return Ok();
        }
    }
}
