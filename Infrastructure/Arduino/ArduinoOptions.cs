
using Infrastructure.Base;

namespace Infrastructure.Arduino
{
    public class ArduinoOptions : LedBase
    {
        /// <summary>
        /// 串口
        /// </summary>
        public string SerialPortName { get; set; }
    }
}