using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using DotNetty.Buffers;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Net;

namespace Infrastructure.Netty.Servser
{
    public class NettyServerFactory
    {
        private ILogger<NettyServerFactory> _logger;
        private NettyServerOptions _optionsMonitor;
        private ServerBootstrap _bootstrap;
        private static object _obj = new object();

        /// <summary>
        /// netty server 客户端接入
        /// </summary>
        public static Action<Hashtable> OnConnectMessage;

        /// <summary>
        /// netty server 客户端断开通知
        /// </summary>
        public static Action<Hashtable> OnCloseConnectMessage;


        /// <summary>
        /// netty server 服务状态通知
        /// </summary>
        public static Action<ServerStatus, string> OnServerStatusMessage;

        /// <summary>
        /// 消息返回通知
        /// </summary>
        public static Action<byte[]> OnReciveMessage;

        public NettyServerFactory(NettyServerOptions optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor;
        }

        public NettyServerFactory(ILogger<NettyServerFactory> logger, IOptionsMonitor<NettyServerOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_bootstrap == null)
            {
                lock (_obj)
                {
                    if (_bootstrap == null)
                    {
                        RunServerAsync();
                    }
                }
            }
        }

        public async Task RunServerAsync()
        {
            IEventLoopGroup bossGroup;//主要工作组，设置为2个线程
            IEventLoopGroup workerGroup;//子工作组，推荐设置为内核数*2的线程数
            bossGroup = new MultithreadEventLoopGroup(1);//主线程只会实例化一个
            workerGroup = new MultithreadEventLoopGroup();//子线程组可以按照自己的需求在构造函数里指定数量

            try
            {
                /*
                *ServerBootstrap是一个引导类，表示实例化的是一个服务端对象
                *声明一个服务端Bootstrap，每个Netty服务端程序，都由ServerBootstrap控制，
                *通过链式的方式组装需要的参数
                */
                _bootstrap = new ServerBootstrap();
                //添加工作组，其中内部实现为将子线程组内置到主线程组中进行管理
                _bootstrap.Group(bossGroup, workerGroup);
                _bootstrap.Channel<TcpServerSocketChannel>();
                _bootstrap
                .Option(ChannelOption.SoBacklog, 100)
                .Option(ChannelOption.SoReuseport, true)//设置端口复用
                .Option(ChannelOption.SoReuseaddr, true)
                .Handler(new LoggingHandler("SRV-LSTN"))//初始化日志拦截器
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>//初始化Tcp服务
{
    /*
    * 这里主要是配置channel中需要被设置哪些参数，以及channel具体的实现方法内容。
    * channel可以理解为，socket通讯当中客户端和服务端的连接会话，会话内容的处理在channel中实现。
    */

    IChannelPipeline pipeline = channel.Pipeline;

    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
    //pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));//Dotnetty自带的编码器，将要发送的内容进行编码然后发送
    //pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));//Dotnetty自带的解码器，将接受到的内容进行解码然后根据内容对应到业务逻辑当中

    pipeline.AddLast("echo", new EchoServerHandler());//server的channel的处理类实现

}));

                IChannel boundChannel = await _bootstrap.BindAsync(IPAddress.Parse(_optionsMonitor.ServerAddress), _optionsMonitor.ServerPort);//指定服务端的端口号，ip地址donetty可以自动获取到本机的地址。也可以在这里手动指定。

                if (OnServerStatusMessage != null)
                    OnServerStatusMessage(ServerStatus.Running, "服务启动成功");

                //Console.ReadLine();
                //await boundChannel.CloseAsync();//关闭
            }
            catch (Exception ex)
            {

                _logger.LogError(ex.Message);
                if (OnServerStatusMessage != null)
                    OnServerStatusMessage(ServerStatus.Error, "服务状态异常 ：" + Environment.NewLine + ex.Message);

                await Task.WhenAll(
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
                if (OnServerStatusMessage != null)
                    OnServerStatusMessage(ServerStatus.Stop, "服务关闭");
            }
            finally
            {
                //关闭释放并退出

            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="context">客户端上下文</param>
        /// <param name="message">发送的消息</param>
        /// <param name="encoding">编码</param>
        public void Write(IChannelHandlerContext context, string message, Encoding encoding)
        {
            try
            {
                IByteBuffer initialMessage = Unpooled.Buffer(1024);
                initialMessage.WriteBytes(encoding.GetBytes(message));
                context.WriteAndFlushAsync(initialMessage).GetAwaiter();
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(nameof(context), "参数context为NULL");
            }

        }

        public void WriteBytes(IChannelHandlerContext context, byte[] message)
        {
            try
            {
                IByteBuffer initialMessage = Unpooled.Buffer(1024);
                initialMessage.WriteBytes(message);
                context.WriteAndFlushAsync(initialMessage).GetAwaiter();
            }
            catch (Exception)
            {
                throw new ArgumentNullException(nameof(context), "参数context为NULL");
            }
        }
    }
}
