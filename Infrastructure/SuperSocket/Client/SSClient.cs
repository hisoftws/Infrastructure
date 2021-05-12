using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Infrastructure.SuperSocket.Client
{
    public class SSClient
    {
        private AsyncTcpSession client = null;
        private string _ip = string.Empty;
        private int _port = 0;
        public Action<string, byte[]> OnReport;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">服务器IP</param>
        /// <param name="port">服务器端口</param>
        public SSClient(string ip, int port)
        {
            _ip = ip;
            _port = port;
            client = new AsyncTcpSession();
            // 连接断开事件
            client.Closed += client_Closed;
            // 收到服务器数据事件
            client.DataReceived += client_DataReceived;
            // 连接到服务器事件
            client.Connected += client_Connected;
            // 发生错误的处理
            client.Error += client_Error;
        }
        void client_Error(object sender, ErrorEventArgs e)
        {
           
            Console.WriteLine(e.Exception.Message);
            Connect();
        }

        void client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine($"{_ip}连接成功");
           
        }

        void client_DataReceived(object sender, DataEventArgs e)
        {
            if (OnReport != null)
                OnReport(_ip , e.Data);
        }

        void client_Closed(object sender, EventArgs e)
        {
            Console.WriteLine($"{_ip}连接断开");
            Connect();
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void Connect()
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));
        }

        /// <summary>
        /// 向服务器发命令行协议的数据
        /// </summary>
        /// <param name="key">命令名称</param>
        /// <param name="data">数据</param>
        public void SendCommand(string key, string data)
        {
            if (client.IsConnected)
            {
                byte[] arr = Encoding.Default.GetBytes(string.Format("{0} {1}", key, data));
                client.Send(arr, 0, arr.Length);
            }
            else
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));
            }
        }

        /// <summary>
        /// 向服务器发命令行协议的数据
        /// </summary>
        /// <param name="key">命令名称</param>
        /// <param name="data">数据</param>
        public void SendCommand(byte[] data)
        {
            if (client.IsConnected)
            {
                client.Send(data, 0, data.Length);
            }
            else
            {
                Connect();
            }
        }
    }
}
