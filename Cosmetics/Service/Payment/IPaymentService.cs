using Cosmetics.DTO.Payment;

namespace Cosmetics.Service.Payment
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentUrlAsync(PaymentRequestDTO request);
        Task<bool> HandlePaymentResponseAsync(PaymentResponseDTO response);
        Task<PaymentResponseDTO> GetPaymentByTransactionIdAsync(string transactionId);
        Task<bool> UpdatePaymentStatusAsync(PaymentResponseDTO payment);
    }
}
