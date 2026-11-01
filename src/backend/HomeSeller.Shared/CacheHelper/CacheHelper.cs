using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HomeSeller.Shared.CacheHelper;

public static class CacheHelper
{
    public static async Task SetCacheAsync<T>(IDatabase redis, ILogger logger, T value, string cacheKey,
        TimeSpan? cacheExpiry)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await redis.StringSetAsync(cacheKey, json, cacheExpiry ?? TimeSpan.FromMinutes(5));
            logger.LogInformation($"Set cache for {cacheKey} to {json}");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }

    public static async Task<T?> GetCacheAsync<T>(IDatabase redis, ILogger logger, string cacheKey)
    {
        var cacheJson = await redis.StringGetAsync(cacheKey);
        if (!cacheJson.HasValue)
        {
            return default;
        }

        logger.LogInformation($"Get cache for {cacheKey}");
        return JsonSerializer.Deserialize<T>((string)cacheJson!);
    }

    public static async Task InvalidateCacheAsync(IDatabase redis, ILogger logger, string cacheKey)
    {
        try
        {
            await redis.KeyDeleteAsync(cacheKey);
            logger.LogInformation($"Invalidate {cacheKey}");
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
    }
}