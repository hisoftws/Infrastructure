using System;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Gpio
{
    public static class GpioServiceCollectionExtensions
    {
        public static IServiceCollection AddGpio(this IServiceCollection services, Action<GpioOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(GpioFactory));

            return services;
        }
    }
}
