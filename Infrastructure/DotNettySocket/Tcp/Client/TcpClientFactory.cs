using Coldairarrow.DotNettySocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DotNettySocket.Tcp.Client
{
    public class TcpClientFactory
    {
        private readonly TcpClientOptions _optionMonitor;
        private static object _obj = new object();
        private ILogger<TcpClientFactory> _logger;
        public Action<byte[], ITcpSocketClient> OnRecvice;
        private Coldairarrow.DotNettySocket.ITcpSocketClient _tcpSocketClient;

        public TcpClientFactory(ILogger<TcpClientFactory> logger, IOptionsMonitor<TcpClientOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_tcpSocketClient == null)
            {
                lock (_obj)
                {
                    if (_tcpSocketClient == null)
                    {
                        InitTcpClient().GetAwaiter();
                    }
                }
            }
        }

        private async Task InitTcpClient()
        {
            _tcpSocketClient = await SocketBuilderFactory.GetTcpSocketClientBuilder(_optionMonitor.TcpClientIp, _optionMonitor.TcpClientPort)
               .SetLengthFieldEncoder(2)
               .SetLengthFieldDecoder(ushort.MaxValue, 0, 2, 0, 2)
               .OnClientStarted(client =>
               {
                   _logger.LogInformation($"客户端启动");
               })
               .OnClientClose(client =>
               {
                   _logger.LogInformation($"客户端关闭");
               })
               .OnException(ex =>
               {
                   _logger.LogInformation($"异常:{ex.Message}");
               })
               .OnRecieve((client, bytes) =>
               {
                   _logger.LogInformation($"客户端:收到数据:{Encoding.UTF8.GetString(bytes)}");
                   if (OnRecvice != null)
                       OnRecvice(bytes, client);
               })
               .OnSend((client, bytes) =>
               {
                   _logger.LogInformation($"客户端:发送数据:{Encoding.UTF8.GetString(bytes)}");
               })
               .BuildAsync();
        }

        public async Task TcpClientSend(ITcpSocketClient client, byte[] bytes)
        {
            if (client != null)
                await client.Send(bytes);
        }

        public void TcpClientClose()
        {
            if (_tcpSocketClient != null)
                _tcpSocketClient.Close();
        }
    }
}
