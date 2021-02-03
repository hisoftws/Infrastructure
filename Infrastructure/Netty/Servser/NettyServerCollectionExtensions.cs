using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Netty.Servser
{
    public static class NettyServerCollectionExtensions
    {
        public static IServiceCollection AddNettyServer(this IServiceCollection services, Action<NettyServerOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(NettyServerFactory));

            return services;
        }
    }
}
