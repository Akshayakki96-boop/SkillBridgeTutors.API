using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SkillBridgeTutors.API.Interfaces;

namespace SkillBridgeTutors.API.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayPalService> _logger;

        public PayPalService(HttpClient httpClient, IConfiguration configuration, ILogger<PayPalService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"];

            var authBytes = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");
            var authHeader = Convert.ToBase64String(authBytes);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal OAuth token request failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, body);
                throw new Exception($"PayPal OAuth token request failed ({(int)response.StatusCode}): {body}");
            }

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
        }

        public async Task<PayPalOrderResult> CreateOrderAsync(decimal amount, string currency)
        {
            var accessToken = await GetAccessTokenAsync();

            var payload = new
            {
                intent = "CAPTURE",
                purchase_units = new object[]
                {
                    new
                    {
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2")
                        }
                    }
                },
                application_context = new
                {
                    return_url = _configuration["PayPal:ReturnUrl"] ?? "https://skillbridgetutors.com/payment/success",
                    cancel_url = _configuration["PayPal:CancelUrl"] ?? "https://skillbridgetutors.com/payment/cancel"
                }
            };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal create-order failed. Status: {StatusCode}, Body: {Body}, Payload: {Payload}",
                    response.StatusCode, body, json);
                throw new Exception($"PayPal create-order failed ({(int)response.StatusCode}): {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var orderId = doc.RootElement.GetProperty("id").GetString() ?? string.Empty;

            var approvalUrl = string.Empty;
            foreach (var link in doc.RootElement.GetProperty("links").EnumerateArray())
            {
                if (link.GetProperty("rel").GetString() == "approve")
                {
                    approvalUrl = link.GetProperty("href").GetString() ?? string.Empty;
                    break;
                }
            }

            return new PayPalOrderResult { OrderId = orderId, ApprovalUrl = approvalUrl };
        }

        public async Task<PayPalCaptureResult> CaptureOrderAsync(string orderId)
        {
            var accessToken = await GetAccessTokenAsync();

            using var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{orderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayPal capture-order failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, body);
                throw new Exception($"PayPal capture-order failed ({(int)response.StatusCode}): {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var status = doc.RootElement.GetProperty("status").GetString() ?? string.Empty;

            var captureId = string.Empty;
            var purchaseUnits = doc.RootElement.GetProperty("purchase_units");
            if (purchaseUnits.GetArrayLength() > 0)
            {
                var captures = purchaseUnits[0].GetProperty("payments").GetProperty("captures");
                if (captures.GetArrayLength() > 0)
                {
                    captureId = captures[0].GetProperty("id").GetString() ?? string.Empty;
                }
            }

            return new PayPalCaptureResult { CaptureId = captureId, Status = status };
        }
    }
}
