using M2Mqtt;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Mqtt
{
    /// <summary>
    /// 
    /// </summary>
    public class MqttOptions
    {

        private List<string> _publishtopics = new List<string>();
        private List<string> _subscribetopics = new List<string>();
        /// <summary>
        /// 发布主题列表
        /// </summary>
        public List<string> PublishTopics { get => _publishtopics; set => _publishtopics = value; }

        /// <summary>
        /// 订阅主题列表
        /// </summary>
        public List<string> SubscribeTopic { get => _subscribetopics; set => _subscribetopics = value; }

        /// <summary>
        /// 设备IP
        /// </summary>
        public string ServerIp { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}
