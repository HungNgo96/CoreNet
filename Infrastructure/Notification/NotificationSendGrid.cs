//using MediatR;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Pacagroup.Ecommerce.Application.Interface.Infrastructure;
//using Pacagroup.Ecommerce.Domain.ValueObjects;
//using Pacagroup.Ecommerce.Infrastructure.Communication.Options;
//using SendGrid;
//using SendGrid.Helpers.Mail;

//namespace Pacagroup.Ecommerce.Infrastructure.Notification
//{
//    public class NotificationSendGrid : INotification
//    {
//        private readonly ILogger<NotificationSendGrid> _logger;
//        private readonly SendgridOptions _options;
//        private readonly ISendGridClient _sendGridClient;

//        public NotificationSendGrid(ILogger<NotificationSendGrid> logger, IOptions<SendgridOptions> options, ISendGridClient sendGridClient)
//        {
//            _logger = logger;
//            _options = options.Value;
//            _sendGridClient = sendGridClient;
//        }

//        public async Task<bool> SendMailAsync(EmailPayload payload, CancellationToken cancellationToken = new ())
//        {
//            SendGridMessage message = BuildMessage(payload);
//            Response? response = await _sendGridClient.SendEmailAsync(message).ConfigureAwait(false);

//            if (response.IsSuccessStatusCode)
//            {
//                _logger.LogInformation($"Email sent to {payload.ToAddress} at {response.Headers.Date}");
//                return true;
//            }

//            _logger.LogError($"Email failed to {payload.ToAddress} with error code {response.StatusCode}");
//            return false;
//        }

//        private SendGridMessage BuildMessage(EmailPayload payload)
//        {
//            SendGridMessage message = new SendGridMessage
//            {
//                From = new EmailAddress(_options.FromEmail, _options.FromUser),
//                Subject = payload.Subject
//            };

//            message.AddContent(MimeType.Text, payload.Body);
//            message.AddTo(new EmailAddress(payload.ToAddress, payload.ToUser));

//            if (_options.SandboxMode)
//            {
//                message.MailSettings = new MailSettings { SandboxMode = new SandboxMode { Enable = true } };
//            }

//            return message;
//        }
//    }
//}
