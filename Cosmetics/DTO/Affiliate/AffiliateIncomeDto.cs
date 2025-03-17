namespace Cosmetics.DTO.Affiliate
{
    public class AffiliateIncomeDto
    {
        public decimal TotalEarnings { get; set; } // Tổng số tiền kiếm được
        public decimal PendingAmount { get; set; } // Số tiền chưa rút
        public decimal WithdrawnAmount { get; set; } // Số tiền đã rút
        public int WeeklyClicks { get; set; } // Lượt click trong tuần
        public int WeeklyConversions { get; set; } // Lượt chuyển đổi trong tuần
        public double ConversionRate { get; set; } // Tỷ lệ chuyển đổi (Conversions / Clicks)
    }
}
