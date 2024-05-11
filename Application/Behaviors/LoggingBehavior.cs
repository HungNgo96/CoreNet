using System.Diagnostics;
using System.Globalization;
using Domain.Core.Extensions;
using Domain.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull, IResult
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
            => _logger = logger;

        public async Task<TResponse> Handle(TRequest request,
                                            RequestHandlerDelegate<TResponse> next,
                                            CancellationToken cancellationToken)
        {
            string requestName = typeof(TRequest).Name;

            _logger.Info(nameof(LoggingBehavior<TRequest, TResponse>), nameof(Handle), $"Starting request {requestName}, {DateTime.Now.ToString("dd-MM-yyyy hh:MM:ss", CultureInfo.InvariantCulture)}");

            var timer = new Stopwatch();
            timer.Start();
            var result = await next();
            timer.Stop();
            var timeTaken = timer.Elapsed.TotalSeconds;

            if (timeTaken > 3) // if the request is greater than 3 seconds, then log the warnings
            {
                _logger.Warning(nameof(LoggingBehavior<TRequest, TResponse>), nameof(Handle), $"The request {requestName} took {timeTaken} seconds.");
            }

            if (result is { Succeeded: false })
            {
                using (LogContext.PushProperty("Error", result.Message, true))
                {
                    _logger.ErrorMediatrResult(nameof(LoggingBehavior<TRequest, TResponse>), nameof(Handle), $"Request failure {requestName} with error, {result.Message} , {DateTime.Now.ToString("dd-MM-yyyy hh:MM:ss", CultureInfo.InvariantCulture)}");
                }
            }

            _logger.Info(nameof(LoggingBehavior<TRequest, TResponse>), nameof(Handle), $"Completed request {requestName}, {DateTime.Now.ToString("dd-MM-yyyy hh:MM:ss", CultureInfo.InvariantCulture)} handled ({timeTaken} seconds)");

            return result;
        }
    }
}
