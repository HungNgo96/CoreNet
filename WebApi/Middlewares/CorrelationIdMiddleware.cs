// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Domain.Core.SharedKernel.Correlation;
using Microsoft.Extensions.Primitives;

namespace WebApi.Middlewares
{
    public class CorrelationIdMiddleware(RequestDelegate next)
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-Id";

        public async Task Invoke(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
        {
            var correlationId = GetCorrelationId(context, correlationIdGenerator);

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append(CorrelationIdHeaderKey, new[] { correlationId.ToString() });
                return Task.CompletedTask;
            });

            await next(context);
        }

        private static StringValues GetCorrelationId(HttpContext context, ICorrelationIdGenerator correlationIdGenerator)
        {
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
            {
                return correlationIdGenerator.Get();
            }

            correlationIdGenerator.Set(correlationId!);
            return correlationId;
        }
    }

    public static class CorrelationIdMiddlewareExtension
    {
        internal static IApplicationBuilder UseCorrelationId(this IApplicationBuilder service)
        {
            return service.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
