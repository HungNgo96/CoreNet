// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//Stock Keeping Unit
namespace Domain.Entities.Products
{
    public record Sku
    {
        private const int DefaultLength = 15;
        private Sku(string value) => Value = value;
        public Sku() { }

        public string? Value { get; init; }

        public static Sku? Create(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return value.Length == DefaultLength ? null : new Sku(value);
        }
    }
}
