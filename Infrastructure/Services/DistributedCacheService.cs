using Domain.Core.AppSettings;
using Domain.Core.SharedKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Infrastructure.Services;

public class FusionCacheService(
    IFusionCache fusionCache,
    ILogger<FusionCacheService> logger,
    IOptions<CacheOptions> cacheOptions) : ICacheService
{
    private const string CacheServiceName = nameof(FusionCacheService);

    private readonly FusionCacheEntryOptions _cacheOptions = new()
    {
        Duration = TimeSpan.FromHours(cacheOptions.Value.AbsoluteExpirationInHours),
        JitterMaxDuration = TimeSpan.FromSeconds(cacheOptions.Value.SlidingExpirationInSeconds)
    };

    public async ValueTask<TItem?> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<TItem>> factory, CancellationToken token)
    {
        var result = await factory.Invoke();

        if (result is null)
        {
            return default;
        }

        return await fusionCache.GetOrSetAsync(cacheKey, await factory.Invoke(), _cacheOptions, token: token);
    }

    public async Task<IReadOnlyList<TItem>> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<IReadOnlyList<TItem>>> factory, CancellationToken token)
    {
        return await fusionCache.GetOrSetAsync(cacheKey, await factory.Invoke(), _cacheOptions, token: token);
    }

    public async Task RemoveAsync(CancellationToken token, params string[] cacheKeys)
    {
        foreach (var cacheKey in cacheKeys)
        {
            logger.LogInformation("----- Removed from {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);
            await fusionCache.RemoveAsync(cacheKey, token: token);
        }
    }
}
