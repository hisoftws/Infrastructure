using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.SerialPort.Microsoft
{
    public static class SerialPortCollectionExtensions
    {
        public static IServiceCollection AddSerialPort(this IServiceCollection services, Action<SerialPortOptions> setupAction)
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
            services.AddSingleton(typeof(SerialPortFactory));

            return services;
        }

    }
}
