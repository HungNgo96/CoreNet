using Contract.IntegrationEvents;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MassTransitController : ControllerBase
    {
        private readonly ILogger<MassTransitController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IBus _bus;

        public MassTransitController(ILogger<MassTransitController> logger, IPublishEndpoint publishEndpoint, IBus bus)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _bus = bus;
        }

        /// <summary>
        /// procedure demo
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Procedure()
        {
            await _publishEndpoint.Publish(new DomainEvent.SmsNotificationEvent()
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
    }
}
