using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Mq
{
    public class MqOptions
    {
        /// 主题
        /// </summary>
        public List<string> Toptic { get; set; }

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

        /// <summary>
        /// 模式
        /// </summary>
        public MQmodel MQmodel { get; set; }
    }

    public enum MQmodel
    {
        Topic = 0,
        Queue = 1
    }
}
