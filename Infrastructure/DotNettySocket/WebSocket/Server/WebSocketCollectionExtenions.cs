using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DotNettySocket.WebSocket.Server
{
    public static class WebSocketCollectionExtenions 
    {
         static IServiceCollection AddWebSocket(this IServiceCollection services, Action<WebSocketOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(WebSocketFactory));

            return services;
        }
    }
}
