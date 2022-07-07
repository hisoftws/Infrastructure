using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Device.Gpio;
using System.Collections;
using System.Device.Pwm;
using System.Threading;

namespace Infrastructure.Gpio
{
    public class GpioFactory
    {
        private readonly GpioOptions _optionsMonitor;
        private static object _obj = new object();
        protected ILogger<GpioFactory> _logger;
        protected GpioController _gpio;
        public Hashtable Leds = Hashtable.Synchronized(new Hashtable());

        public GpioFactory(ILogger<GpioFactory> logger, IOptionsMonitor<GpioOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_gpio == null)
            {
                lock (_obj)
                {
                    if (_gpio == null)
                    {
                        Init();
                    }
                }
            }
        }

        /// <summary>
        /// 初始化Gpio针脚编号方式
        /// </summary>
        public GpioController Init()
        {
            try
            {
                Type type = typeof(GpioOptions);
                var arr = type.GetProperties();
                foreach (var item in arr)
                {
                    switch (item.Name.ToLower())
                    {
                        case "green":
                        case "red":
                        case "yellow":
                        case "blue":
                            Leds.Add(item.Name, item.GetValue(_optionsMonitor, null));
                            break;
                        default:
                            break;
                    }
                }
                _gpio = new GpioController((PinNumberingScheme)_optionsMonitor.scheme);
                return _gpio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 设置pin的模式
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <param name="iomodel"> eg： 0 is input model, 1 is output model, 2 is inputpulldown model, 3 is intputpullup model
        /// </param>
        public bool SetPinMode(int pinNum, int iomodel)
        {
            try
            {
                if (_gpio == null)
                    throw new NullReferenceException("Gpio connect is fail.");

                if (_gpio.IsPinModeSupported(pinNum, (PinMode)iomodel))
                {
                    if (_gpio.IsPinOpen(pinNum))
                        return true;
                    _gpio.OpenPin(pinNum, (PinMode)iomodel);
                    return true;
                }
                else
                {
                    throw new NotSupportedException($"this Pin{pinNum} not supported the Mode");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 写入开关量高低电平
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <param name="level"> eg: true is High level, false is Low level
        /// </param>
        /// <returns></returns>
        public bool DigitalWritePin(int pinNum, bool level)
        {
            try
            {
                if (_gpio == null)
                    throw new NullReferenceException("Gpio connect is fail.");

                if (!_gpio.IsPinOpen(pinNum))
                    _gpio.OpenPin(pinNum);

                var pv = level ? PinValue.High : PinValue.Low;
                _gpio.Write(pinNum, pv);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }

        }

        /// <summary>
        /// 读取开关量
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <returns></returns>
        public int DigitalReadPin(int pinNum)
        {
            try
            {
                if (_gpio == null)
                    throw new NullReferenceException("Gpio connect is fail.");

                if (!_gpio.IsPinOpen(pinNum))
                    _gpio.OpenPin(pinNum);

                var pv = _gpio.Read(pinNum);
                switch (pv.ToString().ToLower())
                {
                    case "high":
                        return 1;
                    case "low":
                        return 0;
                }
                throw new ArgumentException("return value is error.", nameof(pv));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        
        /// <summary>
        /// 关闭Pin
        /// </summary>
        /// <param name="pinNum"></param>
        public void CLosePin(int pinNum)
        {
            try
            {
                if (_gpio.IsPinOpen(pinNum))
                    _gpio.ClosePin(pinNum);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw;
            }
        }
    }
}