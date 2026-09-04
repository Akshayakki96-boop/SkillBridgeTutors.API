using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillBridgeTutors.API.Data;
using SkillBridgeTutors.API.DTOs;
using SkillBridgeTutors.API.Interfaces;
using SkillBridgeTutors.API.Models;

namespace SkillBridgeTutors.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayPalService _payPalService;

        public PaymentsController(ApplicationDbContext context, IPayPalService payPalService)
        {
            _context = context;
            _payPalService = payPalService;
        }

        /// <summary>
        /// Create a PayPal order for a student payment. Returns an approval URL to redirect the payer to.
        /// </summary>
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentOrderDto dto)
        {
            var studentExists = await _context.Students.AnyAsync(s => s.Id == dto.StudentId);
            if (!studentExists)
                return NotFound(new { message = "Student not found." });

            var order = await _payPalService.CreateOrderAsync(dto.Amount, dto.Currency);

            var payment = new Payment
            {
                StudentId = dto.StudentId,
                PayPalOrderId = order.OrderId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Status = PaymentStatus.Created
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new CreatePaymentOrderResponseDto
            {
                PaymentId = payment.Id,
                OrderId = order.OrderId,
                ApprovalUrl = order.ApprovalUrl
            });
        }

        /// <summary>
        /// Capture a previously approved PayPal order to finalize the payment.
        /// </summary>
        [HttpPost("capture")]
        public async Task<IActionResult> Capture([FromBody] CapturePaymentDto dto)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PayPalOrderId == dto.OrderId);
            if (payment == null)
                return NotFound(new { message = "Payment not found." });

            var result = await _payPalService.CaptureOrderAsync(dto.OrderId);

            payment.PayPalCaptureId = result.CaptureId;
            payment.Status = result.Status == "COMPLETED" ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.CompletedAt = payment.Status == PaymentStatus.Completed ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();

            return Ok(new PaymentResponseDto
            {
                PaymentId = payment.Id,
                OrderId = payment.PayPalOrderId,
                CaptureId = payment.PayPalCaptureId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status.ToString()
            });
        }

        /// <summary>
        /// Get all payments made by a student.
        /// </summary>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var payments = await _context.Payments
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.Id,
                    OrderId = p.PayPalOrderId,
                    CaptureId = p.PayPalCaptureId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            return Ok(payments);
        }
    }
}
