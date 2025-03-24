using Cosmetics.DTO.Product;

namespace Cosmetics.DTO.Cart
{
    public class CartDetailDTO
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime? CreatedAt { get; set; }
        public ProductDTO Product { get; set; }
    }
}
