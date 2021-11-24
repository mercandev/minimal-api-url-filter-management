using Microsoft.Extensions.Caching.Redis;
using MinimalApiUrlFilter.Cache;
using MinimalApiUrlFilter.Service;

namespace MinimalApiUrlFilter.Register
{
    public static class RegisterConfiguration
    {
        public static void Register()
        {
            RedisCacheOptions options = new()
            {
                Configuration = "127.0.0.1:6379",
                InstanceName = "master"
            };
            
             var serviceProvider = new ServiceCollection()
            .AddSingleton<IRedisCacheService, RedisCacheService>()
            .AddSingleton<IUrlFilterService, UrlFilterService>()
            .AddSingleton<RedisCacheOptions>(options)
            .AddDistributedRedisCache(option =>
            {
                option.Configuration = "127.0.0.1:6379";
                option.InstanceName = "";
            })
            .BuildServiceProvider();
        }
    }
}
