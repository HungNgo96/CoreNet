// HungNgo96

using System.Net;
using System.Text.Json;
using Common.Extensions;
using Common.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Middlewares
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.ErrorException(className: nameof(GlobalExceptionHandler), methodName: nameof(TryHandleAsync), e: exception);

            HttpResponse response = httpContext.Response;
            response.ContentType = "application/json";
            Result<string> responseModel = Result<string>.Fail(exception.Message, (int)HttpStatusCode.InternalServerError);
            var statusCode = HttpStatusCode.InternalServerError;
            var title = "An unexpected error occurred.";

            switch (exception)
            {
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Forbidden;
                    title = "Access denied.";
                    break;

                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    title = "Resource not found.";
                    break;

                case OperationCanceledException:
                    statusCode = HttpStatusCode.Accepted;
                    title = "Operation canceled.";
                    break;
            }

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            };

            response.StatusCode = (int)statusCode;
            logger.ErrorException(className: nameof(GlobalExceptionHandler), methodName: nameof(TryHandleAsync), message: JsonSerializer.Serialize(problemDetails), e: exception);
            await response.WriteAsJsonAsync(responseModel, cancellationToken);

            return true; // Đã xử lý exception
        }
    }
}
