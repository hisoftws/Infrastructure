using HslCommunication;
using HslCommunication.Profinet.Siemens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Plc.Siemens
{
    public class SiemensPlcFactory
    {
        private ILogger<SiemensPlcFactory> _logger;
        private SiemensPlcFactory _siemenns;
        private SiemensS7Net S7PLC = null;
        private bool PLCstatus = false;
        private SiemensPlcOptions _optionsMonitor;
        private static object _obj = new object();

        public SiemensPlcFactory(ILogger<SiemensPlcFactory> logger, IOptionsMonitor<SiemensPlcOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_siemenns == null)
            {
                try
                {
                    lock (_obj)
                    {
                        if (Register())
                        {
                            if (_siemenns == null)
                            {

                                Init();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public SiemensPlcFactory(SiemensPlcOptions optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor;

            if (_siemenns == null)
            {
                try
                {
                    lock (_obj)
                    {
                        if (Register())
                        {
                            if (_siemenns == null)
                            {

                                Init();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }


        public bool Init()
        {
            if (string.IsNullOrWhiteSpace(_optionsMonitor.Ip))
                throw new ArgumentException("ip is empty.");
            if (string.IsNullOrWhiteSpace(_optionsMonitor.plcMode))
                throw new ArgumentNullException("plcMode is empty.");

            SiemensPLCS type;
            try
            {
                type = (SiemensPLCS)Enum.Parse(typeof(SiemensPLCS), _optionsMonitor.plcMode.ToUpper().Trim());
            }
            catch (Exception ex)
            {
                throw new ArgumentException("plcMode not found in SiemensPLCS enum. " + " support type S1200,S300,S400,S1500,S200Smart,S200");
            }

            // 连接
            if (!System.Net.IPAddress.TryParse(_optionsMonitor.Ip, out System.Net.IPAddress address))
                throw new ArgumentException("ip format error.");

            string RackBox;
            string SlotBox;
            try
            {
                if (S7PLC != null)
                    Close();

                S7PLC = new SiemensS7Net(type);
                S7PLC.IpAddress = _optionsMonitor.Ip;
                S7PLC.Port = _optionsMonitor.Port;
                if (_optionsMonitor.plcMode.ToString().ToUpper() == "S1200" || _optionsMonitor.plcMode.ToString().ToUpper() == "S1500" || _optionsMonitor.plcMode.ToString().ToUpper() == "SMART")
                {
                    RackBox = "0";
                    SlotBox = "1";
                }
                else
                {
                    RackBox = "0";
                    SlotBox = "2";
                }
                S7PLC.Rack = byte.Parse(RackBox);
                S7PLC.Slot = byte.Parse(SlotBox);
                if (!PLCstatus)
                {
                    var connect = S7PLC.ConnectServer();
                    if (connect.IsSuccess)
                    {
                        PLCstatus = true;
                        return true;
                    }
                    else
                    {
                        PLCstatus = false;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        public bool Close()
        {
            if (PLCstatus)
            {
                var result = S7PLC.ConnectClose();
                S7PLC = null;
                PLCstatus = false;
                return result.IsSuccess;
            }
            else
            {
                throw new Exception("plc status is closed.");
            }
        }
        public bool Register()
        {
            if (!HslCommunication.Authorization.SetAuthorizationCode("a6ffdfde-48bd-4684-b37b-5c2fe7c2e4c6"))
                throw new Exception("register Communication.dll fail.");
            else
                return true;
        }


        /// Write plc by data type
        /// </summary>
        /// <param name="area"></param>
        /// <param name="address"></param>
        /// <param name="val"></param>
        /// <param name="enumDataType">datatype</param>
        /// <returns></returns>
        public OperateResult Plc_Writer(string area, string address, object val, EnumDataType enumDataType)
        {
            if (string.IsNullOrWhiteSpace(area))
                throw new ArgumentException("area is empty.");
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("address is empty.");
            if (val == null)
                throw new ArgumentNullException("val is null.");

            var tmpval = val.ToString().ToLower().StartsWith("0x") ? val.ToString().Remove(0, 2) : val.ToString();

            var result = new OperateResult();
            try
            {
                if (!area.Substring(area.Length - 1, 1).Equals("."))
                    area += ".".ToLower();
                switch (enumDataType)
                {
                    case EnumDataType.Byte:
                        result = S7PLC.Write(area + address, byte.Parse(tmpval));
                        break;
                    case EnumDataType.Short:
                        result = S7PLC.Write(area + address, short.Parse(tmpval));
                        break;
                    case EnumDataType.Int:
                        result = S7PLC.Write(area + address, int.Parse(tmpval));
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        /// <summary>
        /// Read plc by data type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="area"></param>
        /// <param name="address"></param>
        /// <param name="enumDataType"></param>
        /// <returns></returns>
        public OperateResult<T> Plc_Read<T>(string area, string address, EnumDataType enumDataType)
        {
            if (string.IsNullOrWhiteSpace(area))
                throw new ArgumentException("area is empty.");
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("address is empty.");

            try
            {
                if (!area.Substring(area.Length - 1, 1).Equals("."))
                    area += ".".ToLower();
                switch (enumDataType)
                {
                    case EnumDataType.Byte:
                        return (OperateResult<T>)(object)S7PLC.ReadByte(area + address);

                    case EnumDataType.Short:
                        return (OperateResult<T>)(object)S7PLC.ReadInt16(area + address);

                    case EnumDataType.Int:
                        return (OperateResult<T>)(object)S7PLC.ReadInt32(area + address);

                    default:
                        throw new ArgumentException("enumDataType not support data type");
                        break;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        public enum EnumDataType
        {
            Byte,
            Short,
            Int
        }
    }
}
