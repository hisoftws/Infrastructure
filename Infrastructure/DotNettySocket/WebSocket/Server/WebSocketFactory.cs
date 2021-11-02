using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Coldairarrow.DotNettySocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DotNettySocket.WebSocket.Server
{
    public class WebSocketFactory
    {
        private readonly WebSocketOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<WebSocketFactory> _logger;
        private IWebSocketServer _webSocketServer;
        private string _connectionID;
        public Action<string> OnRecvice;

        WebSocketFactory(ILogger<WebSocketFactory> logger, IOptionsMonitor<WebSocketOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if(_webSocketServer == null)
            {
                lock (_obj)
                {
                    if(_webSocketServer == null)
                    {
                        InitServer().GetAwaiter();
                    }
                }
            }
        }

        private async Task InitServer()
        {
            try
            {
                _webSocketServer = await SocketBuilderFactory.GetWebSocketServerBuilder(_optionsMonitor.ListenerWebSocketPort)
                .OnConnectionClose((server, connection) =>
                {
                    _connectionID = string.Empty;
                    Console.WriteLine($"连接关闭,连接名[{connection.ConnectionName}],当前连接数:{server.GetConnectionCount()}");
                })
                .OnException(ex =>
                {
                    _connectionID = string.Empty;
                    Console.WriteLine($"服务端异常:{ex.Message}");   
                })
                .OnNewConnection((server, connection) =>
                {
                    connection.ConnectionName = $"名字{connection.ConnectionId}";
                    Console.WriteLine($"新的连接:{connection.ConnectionName},当前连接数:{server.GetConnectionCount()}");
                    _connectionID = connection.ConnectionId;
                })
                .OnRecieve((server, connection, msg) =>
                {
                    Console.WriteLine($"服务端:数据{msg}");
                    if(OnRecvice != null)
                        OnRecvice(msg);
                })
                .OnSend((server, connection, msg) =>
                {
                    Console.WriteLine($"向连接名[{connection.ConnectionName}]发送数据:{msg}");
                })
                .OnServerStarted(server =>
                {
                    Console.WriteLine($"服务启动");
                }).BuildAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace, nameof(ex));
            }
        }

        public void WebSocketClose()
        {
            if (!string.IsNullOrWhiteSpace(_connectionID))
                _webSocketServer.GetConnectionById(_connectionID).Close();
        }

        public void WebSocketSend(string data)
        {
            if (!string.IsNullOrWhiteSpace(_connectionID))
                _webSocketServer.GetConnectionById(_connectionID).Send(data);
            else
                _logger.LogError("连接已被销毁，请重新连接");
        }

    }
}
