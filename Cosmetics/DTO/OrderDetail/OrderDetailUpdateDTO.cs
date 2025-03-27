namespace Cosmetics.DTO.OrderDetail
{
    public class OrderDetailUpdateDTO
    {
        public Guid OrderDetailId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? CommissionAmount { get; set; }
        public Guid? AffiliateProfileId { get; set; }
    }

}