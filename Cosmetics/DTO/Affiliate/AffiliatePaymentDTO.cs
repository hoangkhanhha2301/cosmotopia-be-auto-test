namespace Cosmetics.DTO.Affiliate
{
    public class AffiliatePaymentDTO
    {
        public Guid PaymentId { get; set; }
        public decimal? Amount { get; set; }
        public string Status { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
