namespace Cosmetics.DTO.OrderDetail
{
    public class OrderDetailCreateDTO
    {
        public Guid? OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? CommissionAmount { get; set; }
    }

}