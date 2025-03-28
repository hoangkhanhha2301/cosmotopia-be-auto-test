namespace Cosmetics.DTO.Affiliate
{
    public class ProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductImageUrl { get; set; } // Lấy từ ImageUrls[0]
        public string Name { get; set; }
    }
}
