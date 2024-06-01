// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Products.Commands.CreateProduct;
using Application.Products.Commands.DeleteProduct;
using Application.Products.Commands.UpdateProduct;
using Application.Products.Queries.GetAllProduct;
using Application.Products.Queries.GetProductById;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers
{
    /// <summary>
    /// Product controller
    /// </summary>
    [ApiController]
    public class ProductController : BaseController
    {
        [SwaggerOperation(Summary = "Get all produc")]
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetAllProduct.Query request, CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(request, cancellationToken: cancellationToken).ConfigureAwait(false));
        }

        [SwaggerOperation(Summary = "Get product by id.")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(new GetProductById.Query(id), cancellationToken: cancellationToken).ConfigureAwait(false));
        }

        [SwaggerOperation(Summary = "Create product.")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProduct.Command request,
                                                     [FromHeader(Name = "X-Idempotency-Key")] string requestId,
                                                     CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(requestId, out Guid parseRequestId))
            {
                return BadRequest(Result<bool>.Fail("Missing header X-Idempotency-Key"));
            }

            return Ok(await Mediator.Send(new CreateProduct.Command()
            {
                Name = request.Name,
                Price = request.Price,
                Sku = request.Sku,
                RequestId = parseRequestId
            }, cancellationToken: cancellationToken).ConfigureAwait(false));
        }

        [SwaggerOperation(Summary = "Update product.")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] Guid id,
                                                     [FromBody] UpdateProductCommand.Command request,
                                                     CancellationToken cancellationToken)
        {
            request.SetId(id: id);

            return Ok(await Mediator.Send(request, cancellationToken: cancellationToken).ConfigureAwait(false));
        }

        [SwaggerOperation(Summary = "Delete product.")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync([FromRoute] DeleteProductCommand.Command request,
                                                     CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(request, cancellationToken: cancellationToken).ConfigureAwait(false));
        }
    }
}
