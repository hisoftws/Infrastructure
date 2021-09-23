using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DotNettySocket.Tcp.Server
{
    public static class TcpServerCollectionExtenions
    {
        public static IServiceCollection AddTcpServer(this IServiceCollection services, Action<TcpServerOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(TcpServerFactory));

            return services;
        }
    }
}
