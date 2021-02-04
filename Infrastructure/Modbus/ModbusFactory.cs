using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modbus.Device;
using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO.Ports;

namespace Infrastructure.Modbus
{
    public class ModbusFactory
    {
        public readonly ModbusOptions _optionsMonitor;
        private static object _obj = new object();
        private System.IO.Ports.SerialPort _serialPort;
        public TcpClient client;

        private Parity _parity;
        private StopBits _stopBits;
        private IModbusMaster master;
        private ILogger<ModbusFactory> _logger;

        public ModbusFactory(ModbusOptions optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor;

            if (master == null)
            {
                lock (_obj)
                {
                    if (master == null)
                    {
                        switch (_optionsMonitor.ModbusFormat)
                        {
                            case ModbusTransferFormat.Tcp:
                                InitModbustTcp();
                                break;
                            case ModbusTransferFormat.RTU:
                                InitModbusRtu();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(_optionsMonitor.ModbusFormat));
                        }
                    }
                }
            }
        }

        public ModbusFactory(ILogger<ModbusFactory> logger, IOptionsMonitor<ModbusOptions> optionsMonitor)
        {
            _logger = logger;
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor.CurrentValue;

            if (master == null)
            {
                lock (_obj)
                {
                    if (master == null)
                    {
                        switch (_optionsMonitor.ModbusFormat)
                        {
                            case ModbusTransferFormat.Tcp:
                                InitModbustTcp();
                                break;
                            case ModbusTransferFormat.RTU:
                                InitModbusRtu();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(_optionsMonitor.ModbusFormat));
                        }
                    }
                }
            }
        }

        private bool InitModbustTcp()
        {
            try
            {
                client = new TcpClient(_optionsMonitor.IpAddress, _optionsMonitor.Port);
                master = ModbusIpMaster.CreateIp(client);
                master.Transport.ReadTimeout = _optionsMonitor.ReadTimeout;
                master.Transport.WriteTimeout = _optionsMonitor.WriteTimeout;
                master.Transport.Retries = _optionsMonitor.Retries;
                master.Transport.WaitToRetryMilliseconds = _optionsMonitor.WaitToRetryMilliseconds;
                if (client.Connected)
                {
                    if (_logger != null)
                        _logger.LogInformation("Modbus TCP Device 连接成功!");
                    return true;
                }
                else
                {
                    if (_logger != null)
                        _logger.LogInformation("Modbus TCP Device 连接失败!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.LogError("Modbus TCP Device 连接异常!");
                throw new Exception("InitModbustTcp:" + ex.StackTrace);

            }
        }

        private bool InitModbusRtu()
        {
            try
            {
                switch (_optionsMonitor.Parity)
                {
                    case 0:
                        _parity = Parity.None;
                        break;
                    case 1:
                        _parity = Parity.Odd;
                        break;
                    case 2:
                        _parity = Parity.Even;
                        break;
                    case 3:
                        _parity = Parity.Mark;
                        break;
                    case 4:
                        _parity = Parity.Space;
                        break;
                }

                switch (_optionsMonitor.StopBits)
                {
                    case 0:
                        _stopBits = StopBits.None;
                        break;
                    case 1:
                        _stopBits = StopBits.One;
                        break;
                    case 2:
                        _stopBits = StopBits.Two;
                        break;
                    case 3:
                        _stopBits = StopBits.OnePointFive;
                        break;
                }

                _serialPort = new System.IO.Ports.SerialPort(_optionsMonitor.PortName, _optionsMonitor.BaudRate, _parity, _optionsMonitor.DataBits, _stopBits);
                master = ModbusSerialMaster.CreateRtu(_serialPort);
                master.Transport.ReadTimeout = _optionsMonitor.ReadTimeout;
                master.Transport.WriteTimeout = _optionsMonitor.WriteTimeout;
                master.Transport.Retries = _optionsMonitor.Retries;
                master.Transport.WaitToRetryMilliseconds = _optionsMonitor.WaitToRetryMilliseconds;
                _serialPort.Close();
                _serialPort.Open();
                if (_serialPort.IsOpen)
                {
                    if (_logger != null)
                        _logger.LogInformation("Modbus Device 连接成功!");
                    return true;
                }
                else
                {
                    if (_logger != null)
                        _logger.LogInformation("Modbus Device 连接失败!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("InitModbusRtu:" + ex.StackTrace);
            }
        }

        public void ModbusClose()
        {
            if (_serialPort != null)
                _serialPort.Close();
            if (client != null)
                client.Close();
            if (master != null)
            {
                master.Dispose();
                master = null;
            }
        }

        /// <summary>
        /// 开启数字输入采集
        /// </summary>
        /// <param name="slaveID">从机id</param> 
        /// <param name="startAddress">读取起始位</param>
        /// <param name="numberOfPoints">读取位数</param>
        public bool[] ReadDigitInput(byte slaveID, ushort startAddress, ushort numberOfPoints)
        {
            try
            {
                bool[] Dinputs = master.ReadInputs(slaveID, startAddress, numberOfPoints);
                return Dinputs;
            }
            catch (Exception ex)
            {
                throw new Exception("OpenDigitInputCollection:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 开启模拟量输入采集
        /// </summary>
        /// <param name="slaveID">从机id</param>
        /// <param name="intervalTime">采集间隔时间</param>
        /// <param name="startAddress">读取起始位</param>
        /// <param name="numberOfPoints">读武器位数</param>
        public ushort[] ReadAnalogInput(byte slaveID, ushort startAddress, ushort numberOfPoints)
        {
            try
            {
                ushort[] Ainputs = master.ReadInputRegisters(slaveID, startAddress, numberOfPoints);
                return Ainputs;
            }
            catch (Exception ex)
            {
                throw new Exception("OpenAnalogInputCollection:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 读取DO信号
        /// </summary>
        /// <param name="slaveID">从机id</param> 
        /// <param name="startAddress">读取起始位</param>
        /// <param name="numberOfPoints">读武器位数</param>
        public bool[] ReadCoils(byte slaveID, ushort startAddress, ushort numberOfPoints)
        {
            try
            {
                bool[] Ainputs = master.ReadCoils(slaveID, startAddress, numberOfPoints);
                return Ainputs;
            }
            catch (Exception ex)
            {
                throw new Exception("ReadCoils:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        /// <param name="slaveID">从机id</param> 
        /// <param name="startAddress">读取起始位</param>
        /// <param name="numberOfPoints">读武器位数</param>
        public ushort[] ReadHoldingRegisters(byte slaveID, ushort startAddress, ushort numberOfPoints)
        {
            try
            {
                ushort[] Ainputs = master.ReadHoldingRegisters(slaveID, startAddress, numberOfPoints);
                return Ainputs;
            }
            catch (Exception ex)
            {
                throw new Exception("ReadReadHolding:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 写线圈
        /// </summary>
        /// <param name="slaveID">从机id</param>
        /// <param name="startAddress">起始位置</param>
        /// <param name="value">高低电平</param>
        public void WriteSingleCoil(byte slaveID, ushort startAddress, bool value)
        {
            try
            {
                master.WriteSingleCoil(slaveID, startAddress, value);
            }
            catch (Exception ex)
            {
                throw new Exception("WriteSingleCoil:" + ex.StackTrace);
            }
        }
    }
}
