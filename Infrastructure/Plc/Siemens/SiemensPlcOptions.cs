using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Plc.Siemens
{
    public class SiemensPlcOptions
    {
        public string Ip { get; set; }

        public int Port { get; set; }

        public string plcMode { get; set; }
    }
}
