using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Modbus
{
    public static class ModbusServiceCollectionExtensions
    {
        public static IServiceCollection AddModbus(this IServiceCollection services, Action<ModbusOptions> setupAction)
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
            services.AddSingleton(typeof(ModbusFactory));

            return services;
        }

    }
}
