// HungNgo96

using Domain.Entities.Products;

namespace Application.Responses.Products
{
    public sealed class GetProductResponse
    {
        public string Name { get; private set; } = string.Empty;
        public Money? Price { get; private set; }

        public Sku? Sku { get; private set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime? ModifiedOnUtc { get; set; }
    }
}
