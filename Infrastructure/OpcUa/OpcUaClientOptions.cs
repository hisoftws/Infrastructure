using M2Mqtt;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.OpcUa
{
    /// <summary>
    /// 
    /// </summary>
    public class OpcUaClientOptions
    {

        /// <summary>
        /// Server IP
        /// </summary>
        public string ServerIp { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// 上级节点名称ns=2;s=Channel1.Device1.{}
        /// </summary>
        public string ParentPath { get; set; }

    }
}
