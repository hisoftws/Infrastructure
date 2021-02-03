using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Modbus
{
    public class ModbusBaseOptions
    {
        public ModbusTransferFormat ModbusFormat { get; set; } = ModbusTransferFormat.Tcp;
    }

    public enum ModbusTransferFormat
    {
        /// <summary>
        /// modbus-tcp
        /// </summary>
        Tcp = 0,
        /// <summary>
        /// modbus-rtu
        /// </summary>
        RTU = 1
    }
}
