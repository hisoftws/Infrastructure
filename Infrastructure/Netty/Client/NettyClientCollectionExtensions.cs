using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Netty.Client
{
    public static class  NettyClientCollectionExtensions
    {
        public static IServiceCollection AddNettyClient(this IServiceCollection services, Action<NettyClientOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(NettyClientFactory));

            return services;
        }
    }
}
