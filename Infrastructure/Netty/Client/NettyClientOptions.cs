using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Netty.Client
{
    public class NettyClientOptions
    {
        public string ServerIP { get; set; }

        public int ServerPort { get; set; }
    }
}
