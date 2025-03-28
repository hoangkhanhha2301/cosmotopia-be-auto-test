using Cosmetics.Enum;

namespace Cosmetics.DTO.Affiliate
{
    public class WithdrawalRequestDto
    {
        public decimal Amount { get; set; }
    }


    public class OrderDtotttt
    {
        public Guid OrderId { get; set; }
        public int? CustomerId { get; set; }
        public Guid? AffiliateProfileId { get; set; }
        public decimal? TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime? OrderDate { get; set; }
    }
}
