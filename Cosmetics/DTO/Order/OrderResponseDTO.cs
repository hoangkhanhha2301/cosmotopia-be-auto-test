using Cosmetics.DTO.OrderDetail;

namespace Cosmetics.DTO.Order
{
    public class OrderResponseDTO
    {
        public Guid OrderId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int? SalesStaffId { get; set; }
        public Guid? AffiliateProfileId { get; set; }
        public decimal? TotalAmount { get; set; }
        public bool? Status { get; set; }
        public DateTime? OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
    }



}
