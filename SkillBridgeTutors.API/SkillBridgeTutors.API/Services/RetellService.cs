using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Services
{
    public class RetellService : IRetellService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RetellService> _logger;

        public RetellService(HttpClient httpClient, IConfiguration configuration, ILogger<RetellService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> TriggerOutboundCallAsync(Lead lead)
        {
            var apiKey = _configuration["Retell:ApiKey"];
            var agentId = _configuration["Retell:AgentId"];
            var fromNumber = _configuration["Retell:FromNumber"];

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from_number = fromNumber,
                to_number = lead.Phone,
                agent_id = agentId,
                metadata = new Dictionary<string, string>
                {
                    { "lead_id", lead.Id.ToString() }
                },
                retell_llm_dynamic_variables = new Dictionary<string, string>
                {
                    { "customer_name", lead.ParentName },
                    { "customer_email", lead.Email },
                    { "customer_phone", lead.Phone },
                    { "customer_query", lead.Query }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.retellai.com/v2/create-phone-call", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Retell API error: {Error}", error);
                throw new Exception($"Retell API call failed: {error}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            return doc.RootElement.GetProperty("call_id").GetString() ?? string.Empty;
        }
    }
}
