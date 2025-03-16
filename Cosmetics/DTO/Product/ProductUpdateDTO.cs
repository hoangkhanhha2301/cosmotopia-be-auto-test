using Swashbuckle.AspNetCore.Annotations;

namespace Cosmetics.DTO.Product
{
    public class ProductUpdateDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int? StockQuantity { get; set; }

        [SwaggerSchema(ReadOnly = true)]

        public decimal? CommissionRate { get; set; }

        public bool? IsActive { get; set; }


        public List<string> ImageUrls { get; set; }
    }
}
