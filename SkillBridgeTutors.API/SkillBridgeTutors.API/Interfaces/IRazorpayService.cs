namespace SkillBridgeTutors.API.Interfaces
{
    public class RazorpayOrderResult
    {
        public string OrderId { get; set; } = string.Empty;

        /// <summary>Amount in the smallest currency unit (e.g. pence/cents), as returned by Razorpay.</summary>
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class RazorpayPaymentResult
    {
        public string PaymentId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;

        /// <summary>Razorpay payment status, e.g. "captured", "failed", "authorized".</summary>
        public string Status { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public interface IRazorpayService
    {
        /// <summary>Creates a Razorpay order. Amount is in the major currency unit (e.g. pounds); it is converted to the smallest unit internally.</summary>
        Task<RazorpayOrderResult> CreateOrderAsync(decimal amount, string currency, string receipt);

        /// <summary>Fetches a payment's current status/amount directly from Razorpay (used to confirm capture server-side).</summary>
        Task<RazorpayPaymentResult> GetPaymentAsync(string paymentId);

        /// <summary>Verifies the HMAC-SHA256 signature Razorpay Checkout returns after a successful payment.</summary>
        bool VerifyPaymentSignature(string orderId, string paymentId, string signature);

        /// <summary>Verifies the HMAC-SHA256 signature on an incoming webhook payload.</summary>
        bool VerifyWebhookSignature(string payload, string signature);
    }
}
