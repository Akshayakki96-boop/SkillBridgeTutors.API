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
        public string OrderId { get; set; } = string.Empty;

        /// <summary>URL to redirect the payer to approve the payment (PayPal 'approve' link).</summary>
        public string ApprovalUrl { get; set; } = string.Empty;
    }

    public class CapturePaymentDto
    {
        [Required]
        public string OrderId { get; set; } = string.Empty;
    }

    public class PaymentResponseDto
    {
        public int PaymentId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string? CaptureId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
