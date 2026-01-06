using Microsoft.Extensions.Caching.Distributed;
using Ogx.Shared.Helper.Consts;

namespace Ogx.Shared.Hosting.Limiter;

public sealed class RedisLockService(IDistributedCache cache)
{
    public async Task<bool> TryAcquireAsync(string key, TimeSpan ttl)
    {
        string lockKey = $"lock:{IdentityConsts.SolutionName}:{key}";

        string existing = await cache.GetStringAsync(lockKey);
        if (existing is not null) return false;

        await cache.SetStringAsync(lockKey, "1",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl });

        return true;
    }

    public Task ReleaseAsync(string key) => cache.RemoveAsync(key);
}