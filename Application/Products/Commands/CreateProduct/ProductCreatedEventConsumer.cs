// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Application.Products.Commands.CreateProduct
{
    [ExcludeFromConfigureEndpoints]//https://masstransit.io/documentation/configuration
    public sealed class ProductCreatedEventConsumer
        : IConsumer<ProductCreatedEvent>
    {
        private readonly ILogger<ProductCreatedEventConsumer> _logger;

        public ProductCreatedEventConsumer(ILogger<ProductCreatedEventConsumer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Consume(ConsumeContext<ProductCreatedEvent> context)
        {
            _logger.Info(nameof(ProductCreatedEventConsumer), nameof(Consume), $"Product created: {context.Message.Name}");

            return Task.CompletedTask;
        }
    }

    public class ProductCreatedEventConsumerDefinition
        : ConsumerDefinition<ProductCreatedEventConsumer>
    {
        public ProductCreatedEventConsumerDefinition()
        {
            // override the default endpoint name
            EndpointName = "product-service";

            // limit the number of messages consumed concurrently
            // this applies to the consumer only, not the endpoint
            ConcurrentMessageLimit = 8;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
            IConsumerConfigurator<ProductCreatedEventConsumer> consumerConfigurator, IRegistrationContext context)
        {
            // configure message retry with millisecond intervals
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));
            endpointConfigurator.PrefetchCount = 1;//setup to 0 when you want to consume in order message
            // use the outbox to prevent duplicate events from being published
            endpointConfigurator.UseInMemoryOutbox(context);
        }
    }
}
