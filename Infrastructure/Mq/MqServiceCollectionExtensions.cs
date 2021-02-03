using Infrastructure.Netty.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Mq
{
    public static class MqServiceCollectionExtensions
    {

        public static IServiceCollection AddMqClient(this IServiceCollection services, Action<MqOptions> setupAction)
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