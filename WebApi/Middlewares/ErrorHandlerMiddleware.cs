// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Application.Extensions;
using Domain.Shared;
using System.Net;

namespace WebApi.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        /// <summary>
        /// This is handler middleware
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Hứng data lỗi
        /// </summary>
        /// <param name="context"></param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                await HandleExceptionAsync(context, error);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception error)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/json";
            Result<string> responseModel = Result<string>.Fail(error.Message, (int)HttpStatusCode.InternalServerError);

            _logger.ErrorException(className: nameof(ErrorHandlerMiddleware), methodName: nameof(InvokeAsync), e: error);

            switch (error)
            {
                case UnauthorizedAccessException e:
                    // custom application error
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    responseModel.Code = (int)HttpStatusCode.Forbidden;
                    responseModel.Message = e.Message;
                    break;

                //case ApiException e:
                //    // custom application error
                //    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                //    responseModel.Code = (int)HttpStatusCode.InternalServerError;
                //    responseModel.Message = e.Message;
                //    break;

                case KeyNotFoundException e:
                    // not found error
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    responseModel.Code = (int)HttpStatusCode.NotFound;
                    responseModel.Message = e.Message;
                    break;

                case OperationCanceledException e:
                    // canncellationToken
                    response.StatusCode = (int)HttpStatusCode.Accepted;
                    responseModel.Code = (int)HttpStatusCode.Accepted;
                    responseModel.Message = e.Message;
                    break;

                //case TokenException e:
                //    // canncellationToken
                //    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //    responseModel.Code = (int)HttpStatusCode.Unauthorized;
                //    responseModel.Message = e.Message;
                //    break;

                default:
                    // unhandled error
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    responseModel.Code = (int)HttpStatusCode.InternalServerError;
                    responseModel.Message = error.Message;
                    break;
            }
            await response.WriteAsJsonAsync(responseModel);
        }
    }

    internal static class ErrorHandlerMiddlewareExtension
    {
        /// <summary>
        /// UseErrorHandlerMiddleware
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        internal static IApplicationBuilder UseErrorHandler(this IApplicationBuilder service)
        {
            return service.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}
