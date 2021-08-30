using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.DotNettySocket.Udp
{
    public class UdpOptions
    {
        /// <summary>
        /// 监听upd服务端口
        /// </summary>
        public int ListenerUdpPort { get; set; }

        /// <summary>
        /// 目标upd服务器ip
        /// </summary>
        public string TargetUdpIp { get; set; }

        /// <summary>
        /// 目标upd服务器端口
        /// </summary>
        public int TargetUdpPort { get; set; }
    }
}
