using Coldairarrow.DotNettySocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DotNettySocket.Tcp.Server
{
    public class TcpServerFactory
    {
        private readonly TcpServerOptions _optionMonitor;
        private static object _obj = new object();
        private ILogger<TcpServerFactory> _logger;
        public Action<byte[], ITcpSocketConnection> OnRecvice;
        private Coldairarrow.DotNettySocket.ITcpSocketServer _tcpSocketServer;

        public TcpServerFactory(ILogger<TcpServerFactory> logger, IOptionsMonitor<TcpServerOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_tcpSocketServer == null)
            {
                lock (_obj)
                {
                    if (_tcpSocketServer == null)
                    {
                        InitTcpServer().GetAwaiter();
                    }
                }
            }
        }

        private async Task InitTcpServer()
        {
            _tcpSocketServer = await SocketBuilderFactory.GetTcpSocketServerBuilder(_optionMonitor.ServerPort)
               .SetLengthFieldEncoder(2)
               .SetLengthFieldDecoder(ushort.MaxValue, 0, 2, 0, 2)
               .OnConnectionClose((server, connection) =>
               {
                   _logger.LogError($"连接关闭,连接名[{connection.ConnectionName}],当前连接数:{server.GetConnectionCount()}");
               })
               .OnException(ex =>
               {
                   _logger.LogError($"服务端异常:{ex.Message}");
               })
               .OnNewConnection((server, connection) =>
               {
                   connection.ConnectionName = $"名字{connection.ConnectionId}";
                   _logger.LogInformation($"新的连接:{connection.ConnectionName},当前连接数:{server.GetConnectionCount()}");
               })
               .OnRecieve((server, connection, bytes) =>
               {
                   _logger.LogInformation($"服务端:数据{Encoding.UTF8.GetString(bytes)}");
                   if (OnRecvice != null)
                       OnRecvice(bytes, connection);
               })
               .OnSend((server, connection, bytes) =>
               {
                   _logger.LogInformation($"向连接名[{connection.ConnectionName}]发送数据:{Encoding.UTF8.GetString(bytes)}");
               })
               .OnServerStarted(server =>
               {
                   _logger.LogInformation($"服务启动");
               }).BuildAsync();
        }

        public async Task TcpServerSend(ITcpSocketConnection _tcpSocktConnection, byte[] bytes)
        {
            if (_tcpSocktConnection != null)
            {
                await _tcpSocktConnection.Send(bytes);
            }
            else
            {
                _logger.LogInformation($"tcp connection is broke");
            }
        }

        public void TcpServerClose()
        {
            if (_tcpSocketServer != null)
            {
                _tcpSocketServer.Close();
            }
        }
    }
}
