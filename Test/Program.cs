using System;
using General.Validate.CRC;
using General.Bit;
using Infrastructure.Mqtt.MqttNet;
using System.Threading;
using System.Collections.Generic;
using General.StringBuildPool;
using System.Threading.Tasks;
using General.Log.Log4net;

namespace Test
{
    class Program
    {
        private static readonly log4net.ILog Logger = LogFactory.CreateLogger();

        static void Main(string[] args)
        {
            Logger.Debug("debug");
            Logger.Error("error");
            Logger.Warn("warn");
            Logger.Info("info");

            var options = new MqttOptions
            {
                Account = "admin",
                Password = "password",
                ServerIp = "127.0.0.1",
                ServerPort = 1883,
                QualityOfServiceLevel = 0,
                SubscribeTopic = new List<string> { "test", "test1" }
            };


            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var task = Task.Run(new Action(() =>
            {
                var ct = cancellationToken;

                var factory = new MqttClientFactory(options);
                factory.MqttConnect();

                while (true)
                {
                    factory.Publish("test", new Random().Next(0, 999).ToString());
                    factory.Publish("test1", new Random().Next(0, 999).ToString());
                    Thread.Sleep(1000);

                    if (ct.IsCancellationRequested)
                        break;
                }
            }), cancellationToken);


            var Hexresult = new byte[] { 0xff, 0xa0 }.ToModbusCRC16(true);//General.Validate.CRC.CRC16.ToModbusCRC16(, true);
            var result2 = new byte[] { 0xff, 0xa0 }.ToCRC16(true);//General.Validate.CRC.CRC16.crc16(,2);
            var rec = new byte[] { 0xff, 0xa0 }.crc16();
            var rec1 = Convert.ToInt16(Hexresult, 16);

            int[] ints = new int[] { 1, 1, 1, 0, 0, 0, 0, 0 };
            var result = BitUtil.BitWiter(ints);

            bool[] bools = new bool[] { true, true, true, false, false, false, false, false };
            result = BitUtil.BitWiter(bools);

            bools = BitUtil.GetBitValue(result, 0, readType: -1);

            StringBuilderPool sPoll = new StringBuilderPool(100);
            using (var sb = sPoll.Acquire())
            {
                Console.WriteLine(sb.Item.Append("天地玄黄，宇宙洪荒").ToString());
            }

            while (true)
            {
                var input = Console.ReadLine();
                switch (input)
                {
                    case "close":
                        cancellationTokenSource.Cancel();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
