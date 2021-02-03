using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Mqtt
{
    public static class OpcUaClientCollectionExtensions
    {
        public static IServiceCollection AddMqttClient(this IServiceCollection services, Action<MqttOptions> setupAction)
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
            services.AddSingleton(typeof(MqttClientFactory));

            return services;
        }

    }
}
