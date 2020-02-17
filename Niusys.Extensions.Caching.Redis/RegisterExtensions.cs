using Niusys.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Niusys.Extensions.Caching.Redis
{
    public static class RegisterExtensions
    {
        public static void RegisterCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisSettings>(option =>
            {
                var jsonFileConfig = configuration.GetSection(nameof(RedisSettings));
                option.Host = jsonFileConfig.GetValue<string>(nameof(RedisSettings.Host));
                option.Password = jsonFileConfig.GetValue<string>(nameof(RedisSettings.Password));
            });

            services.AddSingleton<IRedisStore, RedisStore>();
            services.AddSingleton<IRedisClientFactory, RedisClientFactory>();
            services.AddTransient(serviceProvider => ServiceProviderServiceExtensions.GetService<IRedisClientFactory>(serviceProvider).GetDatabase());
            services.AddMemoryCache(setup => { setup.ExpirationScanFrequency = TimeSpan.FromMinutes(1); });
        }
    }
}
