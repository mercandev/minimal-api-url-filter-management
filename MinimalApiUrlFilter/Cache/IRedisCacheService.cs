namespace MinimalApiUrlFilter.Cache
{
    public interface IRedisCacheService
    {
        Task<T?> GetAsync<T>(string key);
        T? Get<T>(string key);
        Task SetAsync(string key, object data, TimeSpan timeSpan);
        Task RemoveAsync(string key);
        void Remove(string key);

        bool IsSet(string key);
    }
}
