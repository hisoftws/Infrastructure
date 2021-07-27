using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Modbus
{
    /// <summary>
    /// modbus 的配置文件
    /// </summary>
    public class ModbusOptions : ModbusBaseOptions
    {
        //byte[] ipAddr, int port, int readTimeout = 100,
        //int writeTimeout = 100, int retries = 3, int waitToRetryMilliseconds = 10
        /// <summary>
        /// IP 地址相关信息
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 串口名
        /// </summary>
        public string PortName { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
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
        /// eg: 0 is None stop bit, 1 is One stop bits, 2 is Two stop bits，3 is OnePointFive bits
        /// </summary>
        public int StopBits { get; set; }

        /// <summary>
        /// 采集频率
        /// </summary>
        public int Rate { get; set; }

        /// <summary>
        /// 读 超时时间
        /// </summary>
        public int ReadTimeout { set; get; } = 100;

        /// <summary>
        /// 写超时时间
        /// </summary>
        public int WriteTimeout { set; get; } = 100;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int Retries { get; set; } = 3;

        /// <summary>
        /// 等待重试毫秒 
        /// </summary>
        public int WaitToRetryMilliseconds { get; set; } = 1000;

        /// <summary>
        /// 从机id
        /// </summary>
        public byte SlaveID { get; set; } = 0x01;

    }
}
