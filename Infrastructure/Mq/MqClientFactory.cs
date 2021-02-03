using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Apache.NMS.ActiveMQ;
using Apache.NMS;
using Microsoft.Extensions.Options;
using Apache.NMS.ActiveMQ.Commands;

namespace Infrastructure.Mq
{
    public class MqClientFactory
    {
        private readonly MqOptions _optionsMonitor;
        private static object _obj = new object();
        private ILogger<MqClientFactory> _logger;
        private IConnectionFactory _connectionFactory;
        private IConnection _connection = null;
        private IMessageProducer _producer = null;
        private Dictionary<string, IMessageProducer> _producers = new Dictionary<string, IMessageProducer>();

        public MqClientFactory(ILogger<MqClientFactory> logger, IOptionsMonitor<MqOptions> optionsMonitor)
        {
            if (optionsMonitor == null)
                throw new ArgumentNullException(nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor.CurrentValue;
            _logger = logger;

            if (_producer == null)
            {
                lock (_obj)
                {
                    if (_producer == null)
                    {
                        Connection();
                    }
                }
            }
        }

        public bool Connection()
        {
            try
            {
                _connectionFactory = new ConnectionFactory($"failover:(tcp://{_optionsMonitor.ServerIp}:{_optionsMonitor.ServerPort})?wireFormat.maxInactivityDuration=0");
                _connection = _connectionFactory.CreateConnection();
                _connection.ClientId = Guid.NewGuid().ToString();
                _connection.ExceptionListener += _connection_ExceptionListener;
                _connection.Start();
                ISession sesison = _connection.CreateSession();
                foreach (var item in _optionsMonitor.Toptic)
                {
                    switch (_optionsMonitor.MQmodel)
                    {
                        case MQmodel.Topic:
                            _producer = sesison.CreateProducer(new ActiveMQTopic(item));
                            break;
                        case MQmodel.Queue:
                            _producer = sesison.CreateProducer(new ActiveMQQueue(item));
                            break;
                    }
                    _producers.Add(item, _producer);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Send(string data, string topic)
        {
            var producer = _producers[topic];
            try
            {
                producer.Send(Encoding.UTF8.GetBytes(data));
                _logger.LogInformation($"Send Data : {data}");
            }
            catch
            {
                _logger.LogError($"Mq Reconnect.");
            }
        }

        private void _connection_ExceptionListener(Exception exception)
        {
            _logger.LogError("生产者发生异常:{0}", exception);
        }
    }
}
