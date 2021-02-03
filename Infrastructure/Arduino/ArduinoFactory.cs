using System;
using System.Collections;
using Arduino4Net.Components.Leds;
using Arduino4Net.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Infrastructure.Arduino
{
    public class ArduinoFactory
    {
        private readonly ArduinoOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<ArduinoFactory> _logger;
        private Arduino4Net.Models.Arduino _session;
        private Arduino4Net.Components.Leds.Led _led;
        public Hashtable Leds = Hashtable.Synchronized(new Hashtable());

        public ArduinoFactory(ILogger<ArduinoFactory> logger, IOptionsMonitor<ArduinoOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_session == null)
            {
                lock (_obj)
                {
                    if (_session == null)
                    {
                        Init();
                    }
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            try
            {
                Type type = typeof(ArduinoOptions);
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
                _session = new Arduino4Net.Models.Arduino(_optionsMonitor.SerialPortName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// Pin工作模式
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <param name="iomodel"> eg： 0 is input, 1 is output, 2 is ANALOG, 3 is PWM, 4 is SERVO </param>
        /// <returns></returns>
        public bool SetPinMode(int pinNum, int iomodel)
        {
            try
            {
                if (_session == null)
                    throw new NullReferenceException("Arduino connect is fail.");

                _session.PinMode(pinNum, (PinMode)iomodel);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 写入开关量电平信号
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <param name="level">eg：ture is HIGH, false is LOW</param>
        /// <returns></returns>
        public bool DigitalWritePin(int pinNum, bool level)
        {
            try
            {
                if (_session == null)
                    throw new NullReferenceException("Arduino connect is fail.");

                var pv = level ? 1 : 0;
                _session.DigitalWrite(pinNum, (DigitalPin)pv);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 写入模拟量
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <param name="value">模拟量值</param>
        /// <returns></returns>
        public bool AnalogWritePin(int pinNum, int value)
        {
            try
            {
                if (_session == null)
                    throw new NullReferenceException("Arduino connect is fail.");

                _session.AnalogWrite(pinNum, value);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 读取模拟量值
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <returns></returns>
        public int AnalogReadPin(int pinNum)
        {
            try
            {
                if (_session == null)
                    throw new NullReferenceException("Arduino connect is fail.");

                return _session.AnalogRead(pinNum);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 读取开关信号
        /// </summary>
        /// <param name="pinNum"></param>
        /// <returns></returns>
        public int DigitalReadPin(int pinNum)
        {
            try
            {
                if (_session == null)
                    throw new NullReferenceException("Arduino connect is fail.");

                return _session.DigitalRead(pinNum);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }

        /// <summary>
        /// 简化LED的控制
        /// 使用前的必要条件PinModel为Output
        /// </summary>
        /// <param name="pinNum">针脚编号</param>
        /// <param name="switchLed">eg: true is on led, false is off led</param>
        public void Led(int pinNum, bool switchLed)
        {
            try
            {
                if (_session == null)
                    throw new NullReferenceException("Arduino connect is fail.");

                _led = new Led(_session, pinNum);
                switch (switchLed)
                {
                    case true:
                        _led.On();
                        break;
                    case false:
                        _led.Off();
                        break;
                }
                _led = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, nameof(ex));
                throw ex;
            }
        }
    }
}
