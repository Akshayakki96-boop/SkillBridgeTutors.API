using System.Text;
using System.Text.Json;
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
        private readonly IRazorpayService _razorpayService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public PaymentsController(ApplicationDbContext context, IRazorpayService razorpayService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _razorpayService = razorpayService;
            _emailService = emailService;
            _configuration = configuration;
        }

        /// <summary>
        /// Create a Razorpay order for a student payment. Returns the order id/amount and the
        /// public Key ID the frontend needs to open Razorpay Checkout.
        /// </summary>
        [HttpPost("create-order")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentOrderDto dto)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == dto.StudentId);
            if (student == null)
                return NotFound(new { message = "Student not found." });

            var payment = new Payment
            {
                StudentId = dto.StudentId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Status = PaymentStatus.Created
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var order = await _razorpayService.CreateOrderAsync(dto.Amount, dto.Currency, receipt: $"payment-{payment.Id}");

            payment.RazorpayOrderId = order.OrderId;
            await _context.SaveChangesAsync();

            return Ok(new CreatePaymentOrderResponseDto
            {
                PaymentId = payment.Id,
                OrderId = order.OrderId,
                Amount = order.Amount,
                Currency = order.Currency,
                KeyId = _configuration["Razorpay:KeyId"] ?? string.Empty,
                StudentName = student.FullName,
                StudentEmail = student.Email,
                StudentPhone = student.Phone
            });
        }

        /// <summary>
        /// Verify a Razorpay Checkout payment (order id, payment id and signature returned by the
        /// Checkout "handler" callback) and finalize the payment.
        /// </summary>
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<IActionResult> Verify([FromBody] VerifyPaymentDto dto)
        {
            var payment = await _context.Payments
                .Include(p => p.Student)
                .AsTracking()
                .FirstOrDefaultAsync(p => p.RazorpayOrderId == dto.RazorpayOrderId);
            if (payment == null)
                return NotFound(new { message = "Payment not found." });

            if (!_razorpayService.VerifyPaymentSignature(dto.RazorpayOrderId, dto.RazorpayPaymentId, dto.RazorpaySignature))
                return BadRequest(new { message = "Payment signature verification failed." });

            // Re-confirm status/amount/currency directly with Razorpay rather than trusting the client.
            var result = await _razorpayService.GetPaymentAsync(dto.RazorpayPaymentId);
            var amountMatches = result.OrderId == dto.RazorpayOrderId && result.Currency == payment.Currency;

            payment.RazorpayPaymentId = result.PaymentId;
            payment.Status = result.Status == "captured" && amountMatches ? PaymentStatus.Completed : PaymentStatus.Failed;
            payment.CompletedAt = payment.Status == PaymentStatus.Completed ? DateTime.UtcNow : null;

            if (payment.Status == PaymentStatus.Completed && payment.Student != null && !payment.Student.IsActive)
            {
                payment.Student.IsActive = true;
            }

            await _context.SaveChangesAsync();

            if (payment.Status == PaymentStatus.Completed && payment.Student != null)
            {
                try
                {
                    await _emailService.SendStudentRegistrationAsync(payment.Student);
                }
                catch
                {
                }
            }

            return Ok(new PaymentResponseDto
            {
                PaymentId = payment.Id,
                OrderId = payment.RazorpayOrderId,
                PaymentReference = payment.RazorpayPaymentId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status.ToString()
            });
        }

        /// <summary>
        /// Mark a payment as cancelled when the payer dismisses Razorpay Checkout without paying.
        /// Idempotent: a no-op if the payment was already completed/finalized by a concurrent
        /// verify call or webhook.
        /// </summary>
        [HttpPost("cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel([FromBody] CancelPaymentDto dto)
        {
            var payment = await _context.Payments.AsTracking().FirstOrDefaultAsync(p => p.RazorpayOrderId == dto.RazorpayOrderId);
            if (payment == null)
                return NotFound(new { message = "Payment not found." });

            if (payment.Status == PaymentStatus.Created || payment.Status == PaymentStatus.Approved)
            {
                payment.Status = PaymentStatus.Cancelled;
                await _context.SaveChangesAsync();
            }

            return Ok(new { paymentId = payment.Id, status = payment.Status.ToString() });
        }

        /// <summary>
        /// Razorpay webhook: a server-to-server safety net that finalizes payments even if the
        /// payer closes the browser before the Checkout "handler" callback runs. Configure this
        /// URL (/api/payments/webhook) and a webhook secret in the Razorpay dashboard, and set the
        /// same secret under Razorpay:WebhookSecret in configuration.
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            Request.EnableBuffering();
            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var payload = await reader.ReadToEndAsync();
            Request.Body.Position = 0;

            var signature = Request.Headers["X-Razorpay-Signature"].ToString();
            if (!_razorpayService.VerifyWebhookSignature(payload, signature))
                return Unauthorized(new { message = "Invalid webhook signature." });

            using var doc = JsonDocument.Parse(payload);
            var eventType = doc.RootElement.TryGetProperty("event", out var eventProp) ? eventProp.GetString() : null;

            if (eventType == "payment.captured")
            {
                var entity = doc.RootElement.GetProperty("payload").GetProperty("payment").GetProperty("entity");
                var orderId = entity.GetProperty("order_id").GetString() ?? string.Empty;
                var paymentId = entity.GetProperty("id").GetString() ?? string.Empty;

                var payment = await _context.Payments
                    .Include(p => p.Student)
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.RazorpayOrderId == orderId);

                if (payment != null && payment.Status != PaymentStatus.Completed)
                {
                    payment.RazorpayPaymentId = paymentId;
                    payment.Status = PaymentStatus.Completed;
                    payment.CompletedAt = DateTime.UtcNow;

                    if (payment.Student != null && !payment.Student.IsActive)
                        payment.Student.IsActive = true;

                    await _context.SaveChangesAsync();

                    if (payment.Student != null)
                    {
                        try
                        {
                            await _emailService.SendStudentRegistrationAsync(payment.Student);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return Ok();
        }

        /// <summary>
        /// Get all payments made by a student.
        /// </summary>
        [HttpGet("student/{studentId}")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            Response.Headers.Pragma = "no-cache";

            var payments = await _context.Payments
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.Id,
                    OrderId = p.RazorpayOrderId,
                    PaymentReference = p.RazorpayPaymentId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(payments);
        }
    }
}
