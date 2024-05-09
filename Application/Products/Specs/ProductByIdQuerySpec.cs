// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Domain.Core.Specification;
using Domain.Entities.Products;

namespace Application.Products.Specs
{
    //public sealed class ProductByIdQuerySpec<TResponse> : SpecificationBase<Product>
    //{
    //    private readonly Guid _id;

    //    public ProductByIdQuerySpec([NotNull] IItemQuery<Guid, TResponse> queryInput)
    //    {
    //        ApplyIncludeList(queryInput.Includes);

    //        _id = queryInput.Id;
    //    }

    //    public override Expression<Func<Product, bool>> Criteria => p => p.Id == _id;
    //}
}
