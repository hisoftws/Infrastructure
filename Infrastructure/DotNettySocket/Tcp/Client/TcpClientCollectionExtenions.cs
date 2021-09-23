using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DotNettySocket.Tcp.Client
{
    public static class TcpClientCollectionExtenions
    {
        public static IServiceCollection AddTcpClient(this IServiceCollection services, Action<TcpClientOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(TcpClientFactory));

            return services;
        }
    }
}
