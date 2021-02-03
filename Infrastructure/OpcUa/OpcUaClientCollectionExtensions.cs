using Infrastructure.SerialPort;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.OpcUa
{
    public static class OpcUaClientCollectionExtensions
    {
        public static IServiceCollection AddOpcUaClientClient(this IServiceCollection services, Action<OpcUaClientOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(OpcUaClientFactory));

            return services;
        }

    }
}
