using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace MinimalApiUrlFilter.Cache
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache distributedCache;
        public RedisCacheService(IDistributedCache distributedCache)
            => this.distributedCache = distributedCache;

        public T? Get<T>(string key)
        {
            string value = distributedCache.GetString(key);

            return (value == null) ? default : JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            string value = await distributedCache.GetStringAsync(key);

            return (value == null) ? default : JsonConvert.DeserializeObject<T>(value);

        }

        public bool IsSet(string key) => distributedCache.Get(key) != null;

        public void Remove(string key)
            => distributedCache.RemoveAsync(key);

        public async Task RemoveAsync(string key)
            => await distributedCache.RemoveAsync(key);


        public async Task SetAsync(string key, object data, TimeSpan timeSpan)
        {
            var option = new DistributedCacheEntryOptions().SetSlidingExpiration(timeSpan);
            option.AbsoluteExpirationRelativeToNow = timeSpan;
            var value = JsonConvert.SerializeObject(data);
            await distributedCache.SetStringAsync(key, value, option);
        }
    }
}
