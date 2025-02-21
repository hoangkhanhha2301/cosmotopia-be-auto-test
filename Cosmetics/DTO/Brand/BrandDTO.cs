namespace Cosmetics.DTO.Brand
{
    public class BrandDTO
    {
        public Guid BrandId { get; set; }

        public string Name { get; set; }

        public bool? IsPremium { get; set; }

        public DateTime? CreatedAt { get; set; }
    }

    public class CreateBrandDTO
    {
        public string Name { get; set; }

        public bool? IsPremium { get; set; }
    }

    public class UpdateBrandDTO
    {
        public string Name { get; set; }

        public bool? IsPremium { get; set; }
    }
}
