using Cosmetics.Enum;

namespace Cosmetics.DTO.Payment
{
    public class PaymentResponseDTO
    {
        public string TransactionId { get; set; } // VNPay transaction ID
        public string? RequestId { get; set; } // VNPay request ID (optional)
        public decimal Amount { get; set; } // Amount paid
        public PaymentStatus Status { get; set; } // Payment status (Pending, Success, Failed)
        public DateTime ResponseTime { get; set; } // Time VNPay sent the response
        public int ResultCode { get; set; } // VNPay result code (e.g., 0 = Success, Others = Failure)
        public string? OrderInfo { get; set; }

        public bool IsSuccess => ResultCode == 0; // Helper to check if payment was successful
    }
}
