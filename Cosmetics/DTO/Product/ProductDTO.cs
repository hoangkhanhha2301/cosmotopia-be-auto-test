using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cosmetics.DTO.Product
{
    public class ProductDTO
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int? StockQuantity { get; set; }

        public string ImageUrls { get; set; }

        public decimal? CommissionRate { get; set; }
        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool? IsActive { get; set; }
    }

    public class CreateProductDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int? StockQuantity { get; set; }

        public decimal? CommissionRate { get; set; }
        public Guid? CategoryId { get; set; }

        public Guid? BrandId { get; set; }

    }

    public class UpdateProductDTO
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int? StockQuantity { get; set; }

        [SwaggerSchema(ReadOnly = true)]
        public string? ImageUrls { get; set; }

        public decimal? CommissionRate { get; set; }

        public bool? IsActive { get; set; }
    }
}
