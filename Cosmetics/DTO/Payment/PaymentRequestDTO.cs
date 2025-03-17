namespace Cosmetics.DTO.Payment
{
    public class PaymentRequestDTO
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string ReturnUrl { get; set; }
        public string PaymentMethod { get; set; }
    }
}
