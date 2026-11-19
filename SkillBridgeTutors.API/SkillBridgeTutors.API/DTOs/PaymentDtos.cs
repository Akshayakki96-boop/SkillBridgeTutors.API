using System.ComponentModel.DataAnnotations;

namespace SkillBridgeTutors.API.DTOs
{
    public class CreatePaymentOrderDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        /// <summary>ISO currency code, e.g. GBP, USD.</summary>
        public string Currency { get; set; } = "GBP";
    }

    public class CreatePaymentOrderResponseDto
    {
        public int PaymentId { get; set; }

        /// <summary>Razorpay order id, passed to Razorpay Checkout on the frontend.</summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>Amount in the smallest currency unit (e.g. pence/cents), as required by Razorpay Checkout.</summary>
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;

        /// <summary>Razorpay public Key ID, used to initialize Razorpay Checkout on the frontend.</summary>
        public string KeyId { get; set; } = string.Empty;

        /// <summary>Prefill details for Razorpay Checkout.</summary>
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentPhone { get; set; } = string.Empty;
    }

    public class VerifyPaymentDto
    {
        [Required]
        public string RazorpayOrderId { get; set; } = string.Empty;

        [Required]
        public string RazorpayPaymentId { get; set; } = string.Empty;

        [Required]
        public string RazorpaySignature { get; set; } = string.Empty;
    }

    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
