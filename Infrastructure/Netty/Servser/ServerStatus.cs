using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Netty.Servser
{
    public enum ServerStatus
    {
        Running = 1,
        Error = -1,
        Stop = 0,
    }
}
