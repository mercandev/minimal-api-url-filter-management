using MinimalApiUrlFilter.Domain;

namespace MinimalApiUrlFilter.Service
{
    public interface IUrlFilterService
    {
        void SetUrlRedis(string key, object data, TimeSpan timeSpan);
        Task<UrlFilterContentModel?> GetUrlRedisAsync<T>(string key);
        bool IsUrlValid(string urlAddress);
        UrlFilterContentModel? GetUrlRedis<T>(string key);
        void RemoveUrlRedis(string key);
        Tuple<string,string> UrlKey(string urlAddress);
        bool IsSet(string key);
    }
}
