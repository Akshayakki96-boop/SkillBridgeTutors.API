using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SkillBridgeTutors.API.Interfaces;

namespace SkillBridgeTutors.API.Services
{
    public class RazorpayService : IRazorpayService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RazorpayService> _logger;

        private string KeyId => _configuration["Razorpay:KeyId"] ?? string.Empty;
        private string KeySecret => _configuration["Razorpay:KeySecret"] ?? string.Empty;
        private string WebhookSecret => _configuration["Razorpay:WebhookSecret"] ?? string.Empty;

        public RazorpayService(HttpClient httpClient, IConfiguration configuration, ILogger<RazorpayService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            var baseUrl = _configuration["Razorpay:BaseUrl"] ?? "https://api.razorpay.com";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{KeyId}:{KeySecret}")));
        }

        public async Task<RazorpayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt)
        {
            // Razorpay expects amounts in the smallest currency unit (e.g. pence for GBP, cents for USD/EUR).
            var amountInSubunit = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

            var payload = new
            {
                amount = amountInSubunit,
                currency,
                receipt,
                payment_capture = 1
            };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/orders");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay create-order failed. Status: {StatusCode}, Body: {Body}, Payload: {Payload}",
                    response.StatusCode, body, json);
                throw new Exception($"Razorpay create-order failed ({(int)response.StatusCode}): {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            return new RazorpayOrderResult
            {
                OrderId = root.GetProperty("id").GetString() ?? string.Empty,
                Amount = root.GetProperty("amount").GetInt64(),
                Currency = root.GetProperty("currency").GetString() ?? currency
            };
        }

        public async Task<RazorpayPaymentResult> GetPaymentAsync(string paymentId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/payments/{paymentId}");

            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Razorpay get-payment failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, body);
                throw new Exception($"Razorpay get-payment failed ({(int)response.StatusCode}): {body}");
            }

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            return new RazorpayPaymentResult
            {
                PaymentId = root.GetProperty("id").GetString() ?? string.Empty,
                OrderId = root.GetProperty("order_id").GetString() ?? string.Empty,
                Status = root.GetProperty("status").GetString() ?? string.Empty,
                Amount = root.GetProperty("amount").GetInt64(),
                Currency = root.GetProperty("currency").GetString() ?? string.Empty
            };
        }

        public bool VerifyPaymentSignature(string orderId, string paymentId, string signature)
        {
            var payload = $"{orderId}|{paymentId}";
            return VerifyHmacSha256(payload, signature, KeySecret);
        }

        public bool VerifyWebhookSignature(string payload, string signature)
        {
            return VerifyHmacSha256(payload, signature, WebhookSecret);
        }

        private static bool VerifyHmacSha256(string payload, string signature, string secret)
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(signature))
                return false;

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
        }
    }
}
