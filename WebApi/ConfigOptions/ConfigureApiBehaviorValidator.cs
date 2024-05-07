using System.Net;
using System.Text.Json;
using Application.Extensions;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.ConfigOptions
{
    public static class ConfigureApiBehaviorExtension
    {
        public static void ConfigureApiBehavior(ApiBehaviorOptions options)
        {
            //options.SuppressInferBindingSourcesForParameters = true;
            //options.SuppressModelStateInvalidFilter = true;
            options.InvalidModelStateResponseFactory = (context) =>
            {
                var messages = new List<string>();
                foreach (var error in context.ModelState.AsEnumerable())
                {
                    foreach (var inner in error.Value?.Errors!)
                    {
                        messages.Add(inner.ErrorMessage);
                    }
                }

                var loggerService = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

                loggerService.Error(nameof(ApiBehaviorOptions), "InvalidModelStateResponseFactory", message: JsonSerializer.Serialize(messages));
                //modify the result
                var result = Result<string>.Fail(messages[0], (int)HttpStatusCode.BadRequest);
                return new BadRequestObjectResult(result);
            };
        }
    }
}
