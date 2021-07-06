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
using flyfire.IO.Ports;
using RJCP.IO.Ports;

namespace Infrastructure.SerialPort.Flyfire
{
    /// <summary>
    /// SerialPortFactory
    /// </summary>
    public class SerialPortFactory
    {

        private readonly SerialPortOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<SerialPortFactory> _logger;
        private SerialPort  _serialPort;

        public SerialPortFactory(SerialPortOptions optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor;

            if (_serialPort == null)
            {
                lock (_obj)
                {
                    if (_serialPort == null)
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

            if (_serialPort == null)
            {
                lock (_obj)
                {
                    if (_serialPort == null)
                    {
                        Init();
                    }
                }
            }
        }
        //初始化串口 步骤 2
        public void Init()
        {
            _serialPort = new SerialPort(_optionsMonitor.PortName, _optionsMonitor.BaudRate, (Parity)_optionsMonitor.Parity, _optionsMonitor.DataBits, (StopBits)_optionsMonitor.StopBits);
            _serialPort.ReceiveTimeout = _optionsMonitor.TimeOut;
            _serialPort.BufSize = _optionsMonitor.BufferSize;
            _serialPort.DtrEnable = _optionsMonitor.DTR == 0 ? false : true;
            _serialPort.RtsEnable = _optionsMonitor.RTS == 0 ? false : true;
            _serialPort.Open();
        }


        /// <summary>
        /// 获取计算机的所有串口 步骤 1
        /// </summary>
        public static string[] GetSerial()
        {
            string[] vs = SerialPort.GetPortNames();
            return vs;
        }

        /// <summary>
        /// 开启监听
        /// </summary>
        /// <param name="receive"></param>
        public void WatchSerialPort(Action<object,byte[]> receive)
        {
            //收到消息时要触发的事件
            _serialPort.ReceivedEvent += new CustomSerialPortReceivedEventHandle((obj, buffers) =>
            {
                if (receive != null)
                    receive(obj, buffers);
            });
            _serialPort.WatchSerialPort();
        }

        //向串口写入数据 步骤 3
        public void Witer(object data, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.STRING:
                    _serialPort.Write(data.ToString());
                    break;
                case DataType.BYTE:
                    _serialPort.Write((byte[])data);
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
            if (_serialPort != null)
            {
                _serialPort.Close();
            }
        }

        private class SerialPort : CustomSerialPort
        {
            public SerialPort(string portName, int baudRate = 115200, Parity parity = Parity.None, int databits = 8, StopBits stopBits = StopBits.One)
                : base(portName, baudRate, parity, databits, stopBits)
            {

            }
            //无意义，只是因为父类的 Sp_DataReceived() 不是 public
            public void WatchSerialPort() => Sp_DataReceived(new object(), new SerialDataReceivedEventArgs(SerialData.Eof));
        }

        public enum DataType
        {
            STRING = 0,
            BYTE = 1
        }
    }
}
