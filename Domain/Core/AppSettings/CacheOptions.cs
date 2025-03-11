using Domain.Core.SharedKernel;

namespace Domain.Core.AppSettings;

public sealed class CacheOptions : IAppOptions
{
    static string IAppOptions.ConfigSectionPath => nameof(CacheOptions);

    public int AbsoluteExpirationInHours { get; init; }
    public int SlidingExpirationInSeconds { get; init; }
}
