using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Plc.Siemens
{
   public static class SiemensPlcCollectionExtensions 
    {
        public static IServiceCollection AddSiemensPlc(this IServiceCollection services, Action<SiemensPlcOptions> setupAction)
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
            services.AddSingleton(typeof(SiemensPlcFactory));

            return services;
        }
    }
}
