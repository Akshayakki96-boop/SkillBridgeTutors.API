namespace SkillBridgeTutors.API.Interfaces
{
    public class PayPalOrderResult
    {
        public string OrderId { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;
    }

    public class PayPalCaptureResult
    {
        public string CaptureId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public interface IPayPalService
    {
        Task<PayPalOrderResult> CreateOrderAsync(decimal amount, string currency);
        Task<PayPalCaptureResult> CaptureOrderAsync(string orderId);
    }
}
