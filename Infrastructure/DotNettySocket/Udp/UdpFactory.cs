using Coldairarrow.DotNettySocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DotNettySocket.Udp
{
    public class UdpFactory
    {
        private readonly UdpOptions _optionMonitor;
        private static object _obj = new object();
        private ILogger<UdpFactory> _logger;
        public Action<byte[]> OnRecvice;
        private IUdpSocket _udpSocketServer;
        private IUdpSocket _udpSocketClient;

        public UdpFactory(ILogger<UdpFactory> logger, IOptionsMonitor<UdpOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_udpSocketServer == null || _udpSocketClient == null)
            {
                lock (_obj)
                {
                    if (_udpSocketServer == null)
                    {
                        if (_optionMonitor.ListenerUdpPort != 0)
                            InitServer().GetAwaiter();
                    }

                    if (_udpSocketClient == null)
                    {
                        if (!string.IsNullOrWhiteSpace(_optionMonitor.TargetUdpIp) && _optionMonitor.TargetUdpPort != 0)
                        {
                            InitClient().GetAwaiter();
                        }
                    }
                }
            }
        }

        private async Task InitServer()
        {
            try
            {
                _udpSocketServer = await SocketBuilderFactory.GetUdpSocketBuilder(_optionMonitor.ListenerUdpPort)
                .OnStarted((server) =>
                {
                    _logger.LogInformation("UdpSocketServer Start...");
                })
                .OnRecieve((server, point, bytes) =>
                {
                    _logger.LogInformation($"From {point.ToString() + ""} recvice data :" + BitConverter.ToString(bytes));
                    if (OnRecvice != null)
                        OnRecvice(bytes);
                })
                .OnClose((server) =>
                {
                    _logger.LogInformation("UdpSocketServer Close...");
                })
                .OnException((ex) =>
                {
                    _logger.LogError(ex.StackTrace);
                })
                .BuildAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace, nameof(ex));
            }
        }

        private async Task InitClient()
        {
            try
            {
                _udpSocketClient = await SocketBuilderFactory.GetUdpSocketBuilder().BuildAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace, nameof(ex));
            }
        }

        public void UdpSend(byte[] data)
        {
            if (_udpSocketClient != null)
                _udpSocketClient.Send(data, new IPEndPoint(IPAddress.Parse(_optionMonitor.TargetUdpIp), _optionMonitor.TargetUdpPort));
        }
    }
}
