using M2Mqtt;
using M2Mqtt.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Infrastructure.Mqtt
{
    /// <summary>
    /// CoApServer
    /// </summary>
    public class MqttClientFactory
    {
        private readonly MqttOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<MqttClientFactory> _logger;
        private MqttClient _mqttClient;
        public List<string> publishTopics { get; set; }
        public List<string> subscriberTopics { get; set; }
        

        public MqttClientFactory(ILogger<MqttClientFactory> logger, IOptionsMonitor<MqttOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_mqttClient == null)
            {
                lock (_obj)
                {
                    if (_mqttClient == null)
                    {
                        MqttConnect();
                    }
                }
            }
        }

        /// <summary>
        /// 连接Mqtt
        /// </summary>
        public void MqttConnect()
        {
            try
            {
                publishTopics = _optionsMonitor.PublishTopics;
                subscriberTopics = _optionsMonitor.SubscribeTopic;
                _mqttClient = new MqttClient(_optionsMonitor.ServerIp, _optionsMonitor.ServerPort, false, null, null, MqttSslProtocols.None);
                // 注册消息接收处理事件，还可以注册消息订阅成功、取消订阅成功、与服务器断开等事件处理函数  
                _mqttClient.MqttMsgPublishReceived += _mqttClient_MqttMsgPublishReceived;
                _mqttClient.MqttMsgSubscribed += _mqttClient_MqttMsgSubscribed;
                _mqttClient.ConnectionClosed += _mqttClient_ConnectionClosed;
                //生成客户端ID并连接服务器  
                string clientId = Guid.NewGuid().ToString();
                _mqttClient.Connect(clientId, _optionsMonitor.Account, _optionsMonitor.Password, false, 0x02, false, null, null, true, 60);
                if (subscriberTopics != null && subscriberTopics.Count > 0)
                    _mqttClient.Subscribe(subscriberTopics.ToArray(), new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Mqtt Link Error : MQTTServerip:{_optionsMonitor.ServerIp}:{_optionsMonitor.ServerPort},account:{_optionsMonitor.Account},password:{_optionsMonitor.Password}");
            }
        }

       

        private void _mqttClient_ConnectionClosed(object sender, EventArgs e)
        {
            _mqttClient = null;
            MqttConnect();
        }

        private void _mqttClient_MqttMsgSubscribed(object sender, M2Mqtt.Messages.MqttMsgSubscribedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _mqttClient_MqttMsgPublishReceived(object sender, M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SendMqtt(string data, string mqtttopic)
        {
            if (!_mqttClient.IsConnected)
            {
                MqttConnect();
                return;
            }
            _mqttClient.Publish(mqtttopic, Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
           // _logger.LogInformation($"Mqtt Send : {data}");
        }


        /// <summary>
        /// 关闭mqtt
        /// </summary>
        public void MqttClose()
        {
            _mqttClient.Disconnect();
        }
    }
}
