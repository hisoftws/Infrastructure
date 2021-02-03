using M2Mqtt;
using M2Mqtt.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpcUaHelper;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.OpcUa
{
    /// <summary>
    /// CoApServer
    /// </summary>
    public class OpcUaClientFactory
    {

        private readonly OpcUaClientOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<OpcUaClientFactory> _logger;
        private OpcUaClient client;
        public OpcUaClientFactory(ILogger<OpcUaClientFactory> logger, IOptionsMonitor<OpcUaClientOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (client == null)
            {
                lock (_obj)
                {
                    if (client == null)
                    {
                        client = new OpcUaClient();
                        OpcUaServerConnect();
                    }
                }
            }
        }
        /// <summary>
        /// 链接OpcUa Server
        /// </summary>
        public void OpcUaServerConnect()
        {
            try
            {
                client.ConnectServer(_optionsMonitor.ServerIp);
                if (client.Connected)
                {
                    _logger.LogInformation("OpcUs Server 连接成功!");

                }
                else
                {
                    _logger.LogInformation($"{DateTime.Now.ToString()}_重新连接OpcUaServer_请查看服务器是否启动或配置是否正确");

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"OpcUa Link Error : OpcUaServerip:{_optionsMonitor.ServerIp}:{_optionsMonitor.ServerPort}");
            }
        }

        /// <summary>
        /// opc 写服务数据
        /// </summary> 
        public int SendDataToService(string path, string tag, string tagvalue)
        {
            if (client.Connected)
            {
                try
                {
                    var tagpath = $"{path}.{tag}";
                    var ab = client.WriteNode<string>(tagpath, tagvalue);
                    return 0;
                }
                catch (Exception ex)
                {

                    _logger.LogError($"链接异常OpcServer;{ex.Message}");
                    //OpcUaServerConnect();
                    return -1;
                }
            }
            else
            {
                _logger.LogError($"链接异常OpcServer;设备未链接成功,正在连接");
                Thread.Sleep(500);
                OpcUaServerConnect();
                return -2;
            }
        }
    }
}
