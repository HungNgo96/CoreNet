// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Metrics;
using Application.UseCases.v1.Products.Commands.CreateProduct;
using Application.UseCases.v1.Products.Commands.DeleteProduct;
using Application.UseCases.v1.Products.Commands.UpdateProduct;
using Application.UseCases.v1.Products.Queries.GetAllProduct;
using Application.UseCases.v1.Products.Queries.GetProductById;
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
        private readonly ILogger<ProductController> _logger;
        private static readonly Meter s_meter = new("MyApp.Metrics");
        private static readonly Counter<long> s_requestCounter = s_meter.CreateCounter<long>("app_get_production");

        /// <summary>
        /// Product controller
        /// </summary>
        public ProductController(ILogger<ProductController> logger)
        {
               _logger = logger;
        }

        [SwaggerOperation(Summary = "Get all product")]
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetAllProduct.Query request, CancellationToken cancellationToken)
        {
            s_requestCounter.Add(1);  // Ghi nhận metric
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

            request.SetRequestId(parseRequestId);

            return Ok(await Mediator.Send(request, cancellationToken: cancellationToken).ConfigureAwait(false));
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
        public async Task<IActionResult> DeleteAsync([FromRoute] Guid id,
                                                     CancellationToken cancellationToken)
        {
            return Ok(await Mediator.Send(new DeleteProductCommand.Command(id), cancellationToken: cancellationToken).ConfigureAwait(false));
        }
    }
}
