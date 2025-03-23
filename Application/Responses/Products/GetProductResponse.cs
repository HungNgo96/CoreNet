// HungNgo96

using System.Text.Json.Serialization;
using Common.Serializations;
using Domain.Entities.Products;

namespace Application.Responses.Products
{
    public sealed class GetProductResponse
    {
        [JsonConverter(typeof(JsonLongConverter))]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public Money? Price { get; set; }

        public Sku? Sku { get; set; }

        public long CreatedOnUtc { get; set; }

        public long? ModifiedOnUtc { get; set; }
    }
}
