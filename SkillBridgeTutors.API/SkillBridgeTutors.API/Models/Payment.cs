namespace SkillBridgeTutors.API.Models
{
    public enum PaymentStatus
    {
        Created,
        Approved,
        Completed,
        Failed,
        Cancelled
    }

    public class Payment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        /// <summary>Razorpay order id (from create-order call).</summary>
        public string RazorpayOrderId { get; set; } = string.Empty;

        /// <summary>Razorpay payment id (once payment is completed).</summary>
        public string? RazorpayPaymentId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "GBP";

        public PaymentStatus Status { get; set; } = PaymentStatus.Created;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
