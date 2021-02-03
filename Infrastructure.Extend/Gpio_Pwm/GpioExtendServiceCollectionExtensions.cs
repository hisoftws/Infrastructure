using System;
using Infrastructure.Gpio;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extend.Gpio_Pwm
{
    public static class GpioExtendServiceCollectionExtensions
    {
        public static IServiceCollection AddGpioExtend(this IServiceCollection services, Action<GpioOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(GpioExtendFactory));

            return services;
        }
    }
}
