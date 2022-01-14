using MinimalApiUrlFilter.Cache;
using MinimalApiUrlFilter.Const;
using MinimalApiUrlFilter.Domain;
using System.Text.RegularExpressions;

namespace MinimalApiUrlFilter.Service
{
    public class UrlFilterService : IUrlFilterService
    {
        private readonly IRedisCacheService redisCacheService;
        public UrlFilterService(IRedisCacheService redisCacheService)
            => this.redisCacheService = redisCacheService;
        
        public void SetUrlRedis(string key, object data, TimeSpan timeSpan)
            => redisCacheService.SetAsync(key, data, timeSpan);
        
        public async Task<UrlFilterContentModel?> GetUrlRedisAsync<T>(string key)
             => await redisCacheService.GetAsync<UrlFilterContentModel?>(key);
        
        public bool IsUrlValid(string urlAddress)
             => Regex.IsMatch(urlAddress, UrlFilterConst.CHECK_URL_IS_VALID_REGEX_PATTERN);

        public UrlFilterContentModel? GetUrlRedis<T>(string key)
             => redisCacheService.Get<UrlFilterContentModel?>(key);

        public void RemoveUrlRedis(string key)
             => redisCacheService.Remove(key);

        public Tuple<string?, string> UrlKey(string urlAddress)
        {
            var separateUrl = Regex.Split(urlAddress, UrlFilterConst.SEPARETE_URL_ADDRESS_REGEX_PATTERN)
              .Where(x => !string.IsNullOrEmpty(x) && !x.Contains(UrlFilterConst.WWW))
              .ToArray();

            var domainKey = string.Format(UrlFilterConst.URL_CACHE_KEY,
               string.Join("", separateUrl?.Skip(1)?.Take(2)?.ToArray()), "*");

            var fullKey = string.Join("", domainKey?.Replace("*", string.Empty),
                separateUrl?.Skip(3)?.Take(20)?.LastOrDefault()?.Replace("-", string.Empty));

            fullKey = fullKey.EndsWith("-") ? fullKey = domainKey : fullKey;
            
            return Tuple.Create(domainKey, fullKey);
        }

        public bool IsSet(string key)
            => redisCacheService.IsSet(key);
    }
}
