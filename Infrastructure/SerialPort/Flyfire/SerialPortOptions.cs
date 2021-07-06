using M2Mqtt;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.SerialPort.Flyfire
{
    /// <summary>
    /// 
    /// </summary>
    public class SerialPortOptions
    {

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        /// <summary>
        /// eg: 0 is none, 1 is odd, 2 is even, 3 is mark, 4 is space
        /// </summary>
        public int Parity { get; set; }

        /// <summary>
        /// eg: databits
        /// </summary>
        public int DataBits { get; set; }

        /// <summary>
        /// eg: 0 is One stop bit, 1 is 1.5 stop bits, 2 is Two stop bits
        /// </summary>
        public int StopBits { get; set; }

        /// <summary>
        /// eg: connections time out
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// eg: size of data
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// eg: 0 is close, 1 is open
        /// </summary>
        public int DTR { get; set; }

        /// <summary>
        /// eg: 0 is close, 1 is open
        /// </summary>
        public int RTS { get; set; }
    }
}
