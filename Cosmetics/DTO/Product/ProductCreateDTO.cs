namespace Cosmetics.DTO.Product
{
    public class ProductCreateDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int? StockQuantity { get; set; }

        public decimal? CommissionRate { get; set; }
        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }
    }
}
