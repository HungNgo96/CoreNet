using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Application.DependencyInjections.Extensions
{
    public static class HttpClientPolicies
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider, int retryCount = 3) =>
            HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(x => x.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                .WaitAndRetryAsync(retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                  + TimeSpan.FromMilliseconds(RandomNumberGeneratorValue()),
                    onRetry: (result, span, index, ctx) =>
                    {
                        var logger = serviceProvider.GetRequiredService<ILogger<HttpResponseMessage>>();
                        logger.LogWarning($"RetryPolicy:: #{index}, received {result.Result.StatusCode} message {result.Result.ReasonPhrase}, retrying...");
                    });

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 3) =>
          HttpPolicyExtensions
              .HandleTransientHttpError()
              .WaitAndRetryAsync(retryCount,
                  retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                + TimeSpan.FromMilliseconds(RandomNumberGeneratorValue()),
                  onRetry: (result, span, index, ctx) =>
                  {
                      Console.WriteLine("RefreshToken::::::::::::::::::::: retrying...{0}", DateTime.Now.ToString("dd-MM-yyyy hhmmss"));
                  });

        public static IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy(IServiceProvider serviceProvider, Func<Context, CancellationToken, Task<HttpResponseMessage>> valueFactory) =>
            HttpPolicyExtensions
            .HandleTransientHttpError()
            .FallbackAsync(valueFactory,
                (res, ctx) =>
                {
                    var logger = serviceProvider.GetService<ILogger<HttpResponseMessage>>();
                    logger?.LogWarning($"returning fallback value...");
                    return Task.CompletedTask;
                });

        private static int RandomNumberGeneratorValue()
        {
            var randomNumberBytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumberBytes);
            // Convert the random bytes to an integer
            var randomNumber = BitConverter.ToInt32(randomNumberBytes, 0);
            return Math.Abs(randomNumber) % 101;
        }
    }
}
