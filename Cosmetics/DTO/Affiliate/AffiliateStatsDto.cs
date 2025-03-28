namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateStatsDto
    {
        public decimal WeeklyEarnings { get; set; }
        public int Count { get; set; }
        public int ConversionCount { get; set; }
        public double ConversionRate => Count > 0 ? (double)ConversionCount / Count : 0;
    }

    public class AffiliateSummaryDto
    {
        public decimal TotalEarnings { get; set; }
        public int TotalClicks { get; set; }
    }
}
