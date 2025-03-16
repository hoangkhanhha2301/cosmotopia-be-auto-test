namespace Cosmetics.DTO.Affiliate
{
    public class TopProductDto
    {
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Revenue { get; set; }
        public int Clicks { get; set; }
        public int TotalOrders { get; set; }
        public double ConversionRate { get; set; }
    }
}
