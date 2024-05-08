// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Products.Commands.CreateProduct;
using Application.Products.Queries;
using Application.Products.Queries.GetProduct;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    public class ProductController : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> GetAllProductAsync([FromQuery] GetAllProduct.Query request, CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(request, cancellationToken: cancellationToken).ConfigureAwait(false));
        }

        [HttpGet]
        public async Task<IActionResult> GetProductByIdAsync([FromQuery] GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(request, cancellationToken: cancellationToken).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductAsync([FromBody] CreateProductCommand request,
                                                            [FromHeader(Name = "X-Idempotency-Key")] string requestId,
                                                            CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(requestId, out Guid parseRequestId))
            {
                return BadRequest(Result<bool>.Fail("Missing header X-Idempotency-Key"));
            }

            return Ok(await Mediator.Send(new CreateProductCommand()
            {
                Id = request.Id,
                Name = request.Name,
                Price = request.Price,
                Sku = request.Sku,
                RequestId = parseRequestId
            }, cancellationToken: cancellationToken).ConfigureAwait(false));
        }
    }
}
