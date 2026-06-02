using Microsoft.Extensions.Caching.Memory;

namespace API.Services.Memory
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _cache;

        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T value))
            {
                return value;
            }

            value = await factory();

            _cache.Set(
                key,
                value,
                expiration ?? TimeSpan.FromMinutes(30));

            return value;
        }
    }
}
