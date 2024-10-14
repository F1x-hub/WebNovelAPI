using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BasicWebNovelAPI.Extensions
{
    public static class RedisExtensions
    {
        public static async Task SetValue<T>(this IDistributedCache cache, string key, T data, 
            TimeSpan? absoluteExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(10)
            };

            var jsonData = JsonSerializer.Serialize(data);

            await cache.SetStringAsync(key, jsonData, options);

        }

        public static async Task<T> GetValue<T>(this IDistributedCache cache, string key)
        {
            var jsonData = await cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(jsonData))
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(jsonData);
        }

    }
}
