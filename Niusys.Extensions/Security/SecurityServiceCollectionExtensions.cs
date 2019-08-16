using Niusys.Security;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SecurityServiceCollectionExtensions
    {
        public static IServiceCollection AddNiusysSecurity(this IServiceCollection services, Action<SecurityOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure(configureOptions);
            services.TryAddSingleton<IEncryptionService, EncryptionService>();
            services.TryAddSingleton<ITokenGenerator, DefaultTokenGenerator>();
            return services;
        }
    }
}
