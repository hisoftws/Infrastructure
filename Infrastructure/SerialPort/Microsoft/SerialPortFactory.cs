using M2Mqtt;
using M2Mqtt.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpcUaHelper;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections.Generic;

namespace Infrastructure.SerialPort.Microsoft
{
    /// <summary>
    /// SerialPortFactory
    /// </summary>
    public class SerialPortFactory
    {

        private readonly SerialPortOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<SerialPortFactory> _logger;
        private System.IO.Ports.SerialPort _port;

        public Action<byte[]> OnRevice;

        public SerialPortFactory(SerialPortOptions optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor;

            if (_port == null)
            {
                lock (_obj)
                {
                    if (_port == null)
                    {
                        Init();
                    }
                }
            }
        }

        public SerialPortFactory(ILogger<SerialPortFactory> logger, IOptionsMonitor<SerialPortOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_port == null)
            {
                lock (_obj)
                {
                    if (_port == null)
                    {
                        Init();
                    }
                }
            }
        }
        //初始化串口
        public void Init()
        {
            try
            {
                _port = new System.IO.Ports.SerialPort(_optionsMonitor.PortName, _optionsMonitor.BaudRate, (Parity)_optionsMonitor.Parity, _optionsMonitor.DataBits, StopBits.One);
                _port.DataReceived += _port_DataReceived;
                if (!_port.IsOpen)
                    _port.Open();
            }
            catch (Exception ex)
            {
                _logger.LogError($"串口初始化出现异常,异常内容:{ex.StackTrace}");
            }
        }

        private void _port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var _port = (System.IO.Ports.SerialPort)sender;
            var buffer = new byte[_port.BytesToRead];
            _port.Read(buffer, 0, buffer.Length);
            if (OnRevice != null)
                OnRevice(buffer);
        }


        /// <summary>
        /// 获取计算机本地的所有串口
        /// </summary>
        public static string[] GetSerial()
        {
            string[] vs = System.IO.Ports.SerialPort.GetPortNames();
            return vs;
        }



        //向串口写入数据
        public void Witer(object data, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.STRING:
                    _port.Write(data.ToString());
                    break;
                case DataType.BYTE:
                    _port.Write((byte[])data, 0, ((byte[])data).Length);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            if (_port != null)
            {
                _port.Close();
            }
        }
        

        public enum DataType
        {
            STRING = 0,
            BYTE = 1
        }
    }
}
