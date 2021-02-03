using System;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Arduino
{
    public static class ArduinoServiceCollectionExtensions
    {
        public static IServiceCollection AddArduino(this IServiceCollection services, Action<ArduinoOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(ArduinoFactory));

            return services;
        }
    }
}
