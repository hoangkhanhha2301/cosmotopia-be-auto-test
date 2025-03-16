namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateLinkDTO
    {
        public Guid AffiliateLinkId { get; set; }
        public Guid? ProductId { get; set; }
        public string UniqueCode { get; set; }
        public int? Clicks { get; set; }
        public decimal? EarnedCommission { get; set; }
    }
}
