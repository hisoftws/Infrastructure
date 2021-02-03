using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Netty.Client
{
    internal class EchoClientHandler : ChannelHandlerAdapter
    {
        public static Action<byte[]> OnRecvicer;
        public static Action<bool> OnExption;
        readonly IByteBuffer initialMessage;
        ILogger<NettyClientFactory> _logger;

        public EchoClientHandler(ILogger<NettyClientFactory> logger) => _logger = logger;
        public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage);

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer byteBuffer)
            {
                var buffer = byteBuffer.Array.Skip(byteBuffer.ArrayOffset).Take(byteBuffer.ReadableBytes).ToArray();

                if (OnRecvicer != null)
                    OnRecvicer(buffer);
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _logger.LogInformation("ExceptionCaught");
            context.CloseAsync();
        }

        public override void HandlerRemoved(IChannelHandlerContext context)
        {
            _logger.LogInformation("HandlerRemoved");
            base.HandlerRemoved(context);
            if (OnExption != null)
                OnExption(true);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            _logger.LogInformation("ChannelInactive");
            base.ChannelInactive(context);
        }
    }
}