namespace AdvancedDevSample.Application.DTOs.Product.ProductResponses
{
    using AdvancedDevSample.Domain.Entities;

    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsActive { get; set; }

        public static ProductResponse FromProduct(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsActive = product.IsActive
            };
        }
    }
}
