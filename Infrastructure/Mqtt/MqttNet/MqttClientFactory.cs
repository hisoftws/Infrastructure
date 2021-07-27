using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Mqtt.MqttNet
{
    public class MqttClientFactory
    {
        private static object _obj = new object();
        private readonly MqttOptions _optionsMonitor;
        private ILogger<MqttClientFactory> _logger;
        private MqttClient _mqttClient;
        private static IMqttClientOptions options = null;


        public MqttClientFactory(MqttOptions optionsMonitor)
        {
            if (optionsMonitor == null)
            {
                throw new ArgumentNullException(nameof(optionsMonitor));
            }

            _optionsMonitor = optionsMonitor;

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

        public MqttClientFactory(IOptionsMonitor<MqttOptions> optionsMonitor, ILogger<MqttClientFactory> logger)
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

        public void MqttConnect()
        {
            var factory = new MqttFactory();        //声明一个MQTT客户端的标准步骤 的第一步
            _mqttClient = factory.CreateMqttClient() as MqttClient;  //factory.CreateMqttClient()实际是一个接口类型（IMqttClient）,这里是把他的类型变了一下
            options = new MqttClientOptionsBuilder()    //实例化一个MqttClientOptionsBulider
                .WithTcpServer(_optionsMonitor.ServerIp, _optionsMonitor.ServerPort)
                .WithCredentials(_optionsMonitor.Account, _optionsMonitor.Password)
                .WithClientId(Guid.NewGuid().ToString())
                .Build();
            _mqttClient.ConnectAsync(options).GetAwaiter();      //连接服务器

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(Connected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(Disconnected);
            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(new Action<MqttApplicationMessageReceivedEventArgs>(MqttApplicationMessageReceived));
        }

        private void Connected(MqttClientConnectedEventArgs e)
        {
            try
            {
                List<MqttTopicFilter> listTopic = new List<MqttTopicFilter>();
                if (listTopic.Count <= 0 && _optionsMonitor.SubscribeTopic.Count > 0)
                {
                    _optionsMonitor.SubscribeTopic.ForEach(item =>
                    {
                        var topicFilterBulder = new TopicFilterBuilder().WithTopic(item).Build();
                        listTopic.Add(topicFilterBulder);
                        Console.WriteLine("Connected >>Subscribe " + item);
                    });

                    _mqttClient.SubscribeAsync(listTopic.ToArray()).GetAwaiter();
                    Console.WriteLine("Connected >>Subscribe Success");
                }
                else
                {
                    Console.WriteLine("Connected >>Subscribe Fail");
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        private void Disconnected(MqttClientDisconnectedEventArgs e)
        {
            try
            {
                Console.WriteLine("Disconnected >>Disconnected Server");
                Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    _mqttClient.ConnectAsync(options).GetAwaiter();
                }
                catch (Exception exp)
                {
                    Debug.WriteLine("Disconnected >>Exception " + exp.Message);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        /// <summary>
        /// 接收消息触发事件
        /// </summary>
        /// <param name="e"></param>
        private static void MqttApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                string text = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                string Topic = e.ApplicationMessage.Topic;
                string QoS = e.ApplicationMessage.QualityOfServiceLevel.ToString();
                string Retained = e.ApplicationMessage.Retain.ToString();
                Debug.WriteLine("MessageReceived >>Topic:" + Topic + "; QoS: " + QoS + "; Retained: " + Retained + ";");
                Debug.WriteLine("MessageReceived >>Msg: " + text);
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
        }

        /// <summary>       
        /// /// 发布        
        /// <paramref name="QoS"/>        
        /// <para>0 - 最多一次</para>        
        /// <para>1 - 至少一次</para>        
        /// <para>2 - 仅一次</para>        
        /// </summary>       
        /// <param name="Topic">发布主题</param>        
        /// <param name="Message">发布内容</param>        
        /// <returns></returns>        
        public void Publish(string Topic, string Message)
        {
            try
            {
                if (_mqttClient == null)
                    return;
                if (_mqttClient.IsConnected == false)
                    _mqttClient.ConnectAsync(options);
                if (_mqttClient.IsConnected == false)
                {
                    Debug.WriteLine("Publish >>Connected Failed! ");
                    return;
                }
                Debug.WriteLine("Publish >>Topic: " + Topic + "; QoS: " + _optionsMonitor.QualityOfServiceLevel + "; Retained: " + _optionsMonitor.Retained + ";");
                Debug.WriteLine("Publish >>Message: " + Message);
                MqttApplicationMessageBuilder mamb = new MqttApplicationMessageBuilder()
                    .WithTopic(Topic)
                    .WithPayload(Message).WithRetainFlag(_optionsMonitor.Retained);
                if (_optionsMonitor.QualityOfServiceLevel == 0)
                {
                    mamb = mamb.WithAtMostOnceQoS();
                }
                else if (_optionsMonitor.QualityOfServiceLevel == 1)
                {
                    mamb = mamb.WithAtLeastOnceQoS();
                }
                else if (_optionsMonitor.QualityOfServiceLevel == 2)
                {
                    mamb = mamb.WithExactlyOnceQoS();
                }
                _mqttClient.PublishAsync(mamb.Build());
            }
            catch (Exception exp)
            {
                Console.WriteLine("Publish >>" + exp.Message);
            }
        }
    }
}
