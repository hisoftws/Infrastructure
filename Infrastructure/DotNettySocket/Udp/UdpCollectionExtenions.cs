using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DotNettySocket.Udp
{
    public static class UdpCollectionExtenions
    {
        public static IServiceCollection AddUdp(this IServiceCollection services, Action<UdpOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(UdpFactory));

            return services;
        }
    }
}
