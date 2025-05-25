using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BasicWebNovelAPI.Extensions
{
    public static class RedisExtensions
    {
        public static async Task SetValue<T>(this IDistributedCache cache, string key, T data, 
            TimeSpan? absoluteExpireTime = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(1)
                };

                var jsonData = JsonSerializer.Serialize(data);

                await cache.SetStringAsync(key, jsonData, options);
            }
            catch
            {
                // Silently continue if cache write fails (read-only Redis)
            }
        }

        public static async Task<T> GetValue<T>(this IDistributedCache cache, string key)
        {
            try
            {
                var jsonData = await cache.GetStringAsync(key);

                if (string.IsNullOrEmpty(jsonData))
                {
                    return default(T);
                }

                return JsonSerializer.Deserialize<T>(jsonData);
            }
            catch
            {
                // Return default value if cache read fails
                return default(T);
            }
        }
        
        // Renamed to SafeRemoveAsync to avoid recursive calls
        public static async Task SafeRemoveAsync(this IDistributedCache cache, string key)
        {
            try
            {
                await cache.RemoveAsync(key);
            }
            catch
            {
                // Silently continue if cache removal fails (read-only Redis)
            }
        }
    }
}
