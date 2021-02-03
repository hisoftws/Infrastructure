using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Netty.Servser
{
    public class NettyServerOptions
    {
        public int ServerPort { get; set; } = 9000;

        public string ServerAddress { get; set; } = "0.0.0.0";
    }
}
