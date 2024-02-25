using System.Globalization;
using Application.Extensions;
using Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
            => _logger = logger;

        public async Task<TResponse> Handle(TRequest request,
                                            RequestHandlerDelegate<TResponse> next,
                                            CancellationToken cancellationToken)
        {
            _logger.Info(nameof(LoggingBehavior<TRequest, TResponse>), nameof(Handle), $"Starting request {typeof(TRequest).Name}, {DateTime.Now.ToString("dd-MM-yyyy hh:MM:ss", CultureInfo.InvariantCulture)}");

            var result = await next();

            if (result is { Succeeded: false })
            {
                _logger.ErrorMediatrResult(nameof(LoggingBehavior<TRequest, TResponse>), nameof(Handle), $"Request failure {typeof(TRequest).Name}, {result.Message} , {DateTime.Now.ToString("dd-MM-yyyy hh:MM:ss", CultureInfo.InvariantCulture)}");
            }

            _logger.Info(nameof(LoggingBehavior<TRequest, TResponse>), nameof(Handle), $"Completed request {typeof(TRequest).Name}, {DateTime.Now.ToString("dd-MM-yyyy hh:MM:ss", CultureInfo.InvariantCulture)}");

            return result;
        }
    }
}
