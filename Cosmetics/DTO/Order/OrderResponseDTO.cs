using Cosmetics.DTO.OrderDetail;
using Cosmetics.DTO.Payment;
using Cosmetics.Enum;
using System.Text.Json.Serialization;

namespace Cosmetics.DTO.Order
{
    public class OrderResponseDTO
    {
        public Guid OrderId { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; }

        public string PhoneNumber { get; set; }
        public int? SalesStaffId { get; set; }
        
        public decimal? TotalAmount { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }

        public DateTime? OrderDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Address { get; set; }
        public List<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
        public List<PaymentTransactionDTO> PaymentTransactions { get; set; } = new List<PaymentTransactionDTO>();
    }



}