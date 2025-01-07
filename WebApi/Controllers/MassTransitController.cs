using Application.UseCases.v1.Products.Commands.CreateProduct;
using Contract.IntegrationEvents;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MassTransitController(
        ILogger<MassTransitController> logger,
        IPublishEndpoint publishEndpoint,
        IBus bus)
        : ControllerBase
    {
        /// <summary>
        /// procedure demo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Procedure()
        {
            await publishEndpoint.Publish(new DomainEvent.SmsNotificationEvent()
            {
                Id = Guid.NewGuid(),
                Description = "Sms",
                Name = "Name sms",
                Type = "SMS",
                TimeStamp = DateTime.Now,
                TransactionId = Guid.NewGuid()
            });

            return Accepted();
        }

        [HttpGet]
        public async Task<IActionResult> ProcedureReceiveEndpoint(CancellationToken cancellationToken)
        {
            await bus.Publish(new ProductReceiveEndpoint()
            {
                Id = Guid.NewGuid(),
                Description = "Sms",
                Name = "Name sms",
                Type = "SMS",
                TimeStamp = DateTime.Now,
                TransactionId = Guid.NewGuid()
            }, typeof(ProductReceiveEndpoint), cancellationToken);

            return Accepted();
        }

        [HttpGet]
        public async Task<IActionResult> ProductCreatedEvent(CancellationToken cancellationToken)
        {
            await bus.Publish(new ProductCreatedEvent()
            {
                Id = Guid.NewGuid(),
                Name = "Name ProductCreatedEvent",
                Price = 0
            }, typeof(ProductCreatedEvent), cancellationToken);

            return Accepted();
        }
    }
}
