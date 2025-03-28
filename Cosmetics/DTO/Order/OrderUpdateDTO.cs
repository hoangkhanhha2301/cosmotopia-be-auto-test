using Cosmetics.Enum;

namespace Cosmetics.DTO.Order
{
    public class OrderUpdateDTO
    {
        public Guid OrderId { get; set; }
     
        public OrderStatus Status { get; set; }
        
    }

}