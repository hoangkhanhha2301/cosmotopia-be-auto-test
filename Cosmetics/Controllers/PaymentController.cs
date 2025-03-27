using Cosmetics.DTO.Payment;
using Cosmetics.Enum;
using Cosmetics.Service.Payment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Cosmetics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly string _vnpayHashSecret;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _vnpayHashSecret = configuration["VNPay:HashSecret"] ?? throw new ArgumentNullException("VNPay HashSecret is not configured.");
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var paymentUrl = await _paymentService.CreatePaymentUrlAsync(request);
                return Ok(new { PaymentUrl = paymentUrl });
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging service
                return StatusCode(500, $"An error occurred while creating the payment URL: {ex.Message}");
            }
        }

        [HttpGet("payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var vnpParams = Request.Query.ToDictionary(q => q.Key, q => q.Value.FirstOrDefault());
            var secureHash = vnpParams["vnp_SecureHash"];
            vnpParams.Remove("vnp_SecureHash");

            // Generate checksum to validate the response
            var queryString = string.Join("&", vnpParams
                .OrderBy(k => k.Key)
                .Select(k => $"{k.Key}={Uri.EscapeDataString(k.Value)}"));
            var signData = Encoding.UTF8.GetBytes(queryString);
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_vnpayHashSecret));
            var computedHash = BitConverter.ToString(hmac.ComputeHash(signData)).Replace("-", "").ToLower();

            if (secureHash != computedHash)
                return BadRequest("Invalid checksum");

            // Map VNPay response to PaymentResponseDTO
            var response = new PaymentResponseDTO
            {
                TransactionId = vnpParams["vnp_TxnRef"],
                Amount = decimal.Parse(vnpParams["vnp_Amount"]) / 100, // VNPay returns amount in cents
                ResultCode = int.Parse(vnpParams["vnp_ResponseCode"]),
                ResponseTime = DateTime.ParseExact(vnpParams["vnp_PayDate"], "yyyyMMddHHmmss", null),
                Status = vnpParams["vnp_ResponseCode"] switch
                {
                    "00" => PaymentStatus.Success,
                    "07" => PaymentStatus.Canceled,
                    "09" => PaymentStatus.Processing,
                    _ => PaymentStatus.Failed
                }
            };

            try
            {
                var success = await _paymentService.HandlePaymentResponseAsync(response);
                if (!success)
                    return BadRequest("Failed to process payment response");

                // Redirect based on payment status
                return response.Status switch
                {
                    PaymentStatus.Success => Redirect("/payment/success"),
                    PaymentStatus.Canceled => Redirect("/payment/canceled"),
                    PaymentStatus.Processing => Redirect("/payment/processing"),
                    _ => Redirect("/payment/failed")
                };
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging service
                return StatusCode(500, $"An error occurred while processing the payment response: {ex.Message}");
            }
        }

        [HttpGet("payment/{transactionId}")]
        public async Task<IActionResult> GetPayment(string transactionId)
        {
            if (transactionId == string.Empty) 
                return BadRequest("Transaction ID is required");

            try
            {
                var payment = await _paymentService.GetPaymentByTransactionIdAsync(transactionId);
                if (payment == null)
                    return NotFound($"No payment found with Transaction ID: {transactionId}");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                // Log the exception if you have a logging service
                return StatusCode(500, $"An error occurred while retrieving the payment: {ex.Message}");
            }
        }
        [HttpPut("update-payment-status/{transactionId}")]
        public async Task<IActionResult> UpdatePaymentStatus(string transactionId, [FromQuery] int newStatus)
        {
            if (string.IsNullOrEmpty(transactionId))
                return BadRequest("Transaction ID is required");

            // Kiểm tra status đầu vào
            if (!System.Enum.IsDefined(typeof(PaymentStatus), newStatus) ||
                (newStatus != (int)PaymentStatus.Success && newStatus != (int)PaymentStatus.Failed))
            {
                return BadRequest("New status must be either Success (1) or Fail (2)");
            }

            try
            {
                var payment = await _paymentService.GetPaymentByTransactionIdAsync(transactionId);
                if (payment == null)
                    return NotFound($"No payment found with Transaction ID: {transactionId}");

                if (payment.Status != PaymentStatus.Pending)
                    return BadRequest($"Payment status can only be updated from Pending (0) to Success (1) or Fail (2). Current status: {payment.Status}");

                var updatedPayment = new PaymentResponseDTO
                {
                    TransactionId = payment.TransactionId,
                    Amount = payment.Amount,
                    ResultCode = payment.ResultCode,
                    ResponseTime = DateTime.UtcNow,
                    Status = (PaymentStatus)newStatus // Sử dụng status từ request
                };

                var success = await _paymentService.UpdatePaymentStatusAsync(updatedPayment);
                if (!success)
                    return BadRequest($"Failed to update payment status. Either the status is invalid or the transaction cannot be updated.");

                string statusMessage = updatedPayment.Status == PaymentStatus.Success
                    ? "Success (1)"
                    : "Fail (2)";
                return Ok(new
                {
                    Message = $"Payment status updated to {statusMessage} for Transaction ID: {transactionId}",
                    UpdatedStatus = updatedPayment.Status,
                    ResponseTime = updatedPayment.ResponseTime
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while updating the payment status: {ex.Message}");
            }
        }

    }

}