using DotNetty.Buffers;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace Infrastructure.Netty.Client
{
    public class NettyClientFactory
    {
        private readonly NettyClientOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<NettyClientFactory> _logger;
        private Bootstrap _bootstrap;
        private IChannel _clientChannel;
        private MultithreadEventLoopGroup group;
        private Ping _ping;
        public Action<byte[]> OnReveicer;

        public NettyClientFactory(ILogger<NettyClientFactory> logger, IOptionsMonitor<NettyClientOptions> optionsMonitor)
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
                        Init();
                    }
                }
            }
        }
        public void Init()
        {
            try
            {
                group = new MultithreadEventLoopGroup();

                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true)
                    .Option(ChannelOption.SoBacklog, 128)
                    .Option(ChannelOption.SoKeepalive, true)
                    .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(3))
                    .Handler(
                        new ActionChannelInitializer<ISocketChannel>(
                            channel =>
                            {
                                IChannelPipeline pipeline = channel.Pipeline;
                                pipeline.AddLast(new LoggingHandler());

                                pipeline.AddLast("echo", new EchoClientHandler(_logger));
                            }));
                EchoClientHandler.OnRecvicer = new Action<byte[]>((bytes) =>
                {
                    if (OnReveicer != null)
                        OnReveicer(bytes);
                });
                EchoClientHandler.OnExption = new Action<bool>((flag) =>
                {
                    if (flag == true)
                    {
                        _logger.LogInformation("触发断线重连");
                        ConnectToServer();
                    }
                });
                _bootstrap = bootstrap;
                ConnectToServer();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.StackTrace, nameof(ex));
            }
        }


        private async Task ConnectToServer()
        {
            try
            {
                if (_clientChannel != null && _clientChannel.Active)
                {
                    return;
                }
                _clientChannel = await _bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_optionsMonitor.ServerIP), _optionsMonitor.ServerPort));
                _logger.LogInformation($"连接{_optionsMonitor.ServerIP}:{_optionsMonitor.ServerPort}服务完成");
            }
            catch
            {
                Thread.Sleep(5 * 1000);
                await ConnectToServer();
            }
        }

        public void Reconnect()
        {
            new Thread(() =>
            {
                _ping = new Ping();
                while (true)
                {
                    var reply = _ping.Send(IPAddress.Parse(_optionsMonitor.ServerIP));
                    if (reply.Status != IPStatus.Success)
                    {
                        if (_clientChannel != null)
                            _clientChannel = null;
                    }
                    else
                    {
                        if (_clientChannel == null)
                        {
                            lock (_obj)
                            {
                                ConnectToServer();
                            }
                        }
                    }
                    Thread.Sleep(5 * 1000);
                }
            }).Start();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(string msg)
        {
            List<byte> bytes = Encoding.ASCII.GetBytes(msg).ToList();
            //报头
            bytes.Insert(0, 0x02);
            //包尾
            bytes.Add(0x03);
            byte[] buffer = bytes.ToArray();
            IByteBuffer initialMessage = Unpooled.Buffer(buffer.Length);
            initialMessage.WriteBytes(buffer);
            if (_clientChannel != null && _clientChannel.Active)
                _clientChannel.WriteAndFlushAsync(initialMessage).GetAwaiter();
            else
                _logger.LogError("对象为空", nameof(_clientChannel));
        }
    }
}
