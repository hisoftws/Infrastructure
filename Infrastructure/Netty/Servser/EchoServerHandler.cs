using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections;
using System.Linq;


namespace Infrastructure.Netty.Servser
{
    public class EchoServerHandler : ChannelHandlerAdapter
    {

        public static Hashtable _connectInstance = Hashtable.Synchronized(new Hashtable());
     
        /*
         * Channel的生命周期
         * 1.ChannelRegistered 先注册
         * 2.ChannelActive 再被激活
         * 3.ChannelRead 客户端与服务端建立连接之后的会话（数据交互）
         * 4.ChannelReadComplete 读取客户端发送的消息完成之后
         * error. ExceptionCaught 如果在会话过程当中出现dotnetty框架内部异常都会通过Caught方法返回给开发者
         * 5.ChannelInactive 使当前频道处于未激活状态
         * 6.ChannelUnregistered 取消注册
         */


        /// <summary>
        /// 频道注册
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelRegistered(IChannelHandlerContext context)
        {
            base.ChannelRegistered(context);
        }

        /// <summary>
        /// socket client 连接到服务端的时候channel被激活的回调函数
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelActive(IChannelHandlerContext context)
        {
            //一般可用来记录连接对象信息
            base.ChannelActive(context);
            if (!_connectInstance.ContainsKey(RegexIP(context.Channel.RemoteAddress.ToString())))
            {
                _connectInstance.Add(RegexIP(context.Channel.RemoteAddress.ToString()),context);
                if (NettyServerFactory.OnConnectMessage != null)
                    NettyServerFactory.OnConnectMessage(_connectInstance);
            }
        }

        /// <summary>
        /// socket接收消息方法具体的实现
        /// </summary>
        /// <param name="context">当前频道的句柄，可使用发送和接收方法</param>
        /// <param name="message">接收到的客户端发送的内容</param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer byteBuffer)
            {
                var buffer = byteBuffer.Array.Skip(byteBuffer.ArrayOffset).Take(byteBuffer.ReadableBytes).ToArray();
                if (NettyServerFactory.OnReciveMessage != null)
                    NettyServerFactory.OnReciveMessage(buffer);
            }
        }

        /// <summary>
        /// 该次会话读取完成后回调函数
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();//将WriteAsync写入的数据流缓存发送出去

        /// <summary>
        /// 异常捕获
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }

        /// <summary>
        /// 当前频道未激活状态
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }

        /// <summary>
        /// 取消注册当前频道，可理解为销毁当前频道
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelUnregistered(IChannelHandlerContext context)
        {
            base.ChannelUnregistered(context);
            if (_connectInstance.ContainsKey(RegexIP(context.Channel.RemoteAddress.ToString())))
            {
                _connectInstance.Remove(RegexIP(context.Channel.RemoteAddress.ToString()));
                if (NettyServerFactory.OnCloseConnectMessage != null)
                    NettyServerFactory.OnCloseConnectMessage(_connectInstance);
            }
        }

        private string RegexIP(string str)
        {
            System.Text.RegularExpressions.Match m = System.Text.RegularExpressions.Regex.Match(str, @"\b(([01]?\d?\d|2[0-4]\d|25[0-5])\.){3}([01]?\d?\d|2[0-4]\d|25[0-5])\b");

            if (m.Success)
                return m.Groups[0].Value;
            else
                return "";
        }
    }
}