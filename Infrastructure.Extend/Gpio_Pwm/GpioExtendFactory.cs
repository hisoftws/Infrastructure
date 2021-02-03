using Infrastructure.Gpio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Device.Pwm.Drivers;
using System.Threading;

namespace Infrastructure.Extend.Gpio_Pwm
{
    public class GpioExtendFactory : GpioFactory
    {
        SoftwarePwmChannel pwmChannel = null;
        public GpioExtendFactory(ILogger<GpioFactory> logger, IOptionsMonitor<GpioOptions> optionsMonitor)
            : base(logger, optionsMonitor)
        {

        }

        /// <summary>
        /// PwmChannel 对象
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <param name="frequency">频率，单位为 Hz</param>
        /// <param name="dutyCycle">占空比，取值为 0.0 - 1.0</param>
        public void Pwm(int pinNum, int frequency = 400, double dutyCycle = 0.5)
        {
            if (_gpio == null)
                throw new NullReferenceException("Gpio connect is fail.");

            try
            {
                using (pwmChannel = new SoftwarePwmChannel(pinNum, usePrecisionTimer: true))
                {
                    pwmChannel.Frequency = frequency;
                    pwmChannel.Start();
                 
                    for (double fill = 0.0; fill <= dutyCycle; fill += 0.01)
                    {
                        pwmChannel.DutyCycle = fill;
                        Thread.Sleep(50);
                    }
                    for (double fill = dutyCycle; fill >= 0.00; fill -= 0.01)
                    {
                        pwmChannel.DutyCycle = fill;
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }
    }
}