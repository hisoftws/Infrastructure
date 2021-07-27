using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace General.Log.Log4net
{
    /// <summary>
    /// 日志工厂
    /// </summary>
    public class LogFactory
    {
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static LogFactory()
        {
            var element = CreateConfigElement();//创建XmlElement
            log4net.Config.XmlConfigurator.Configure(element);//配置log4net
        }

        /// <summary>
        /// 创建一个ILog，用于记录日志
        /// </summary>
        /// <returns></returns>
        public static log4net.ILog CreateLogger()
        {
            StackTrace st = new StackTrace();
            var arr = st.GetFrame(1);
            return log4net.LogManager.GetLogger(arr.GetMethod().DeclaringType);
        }
        /// <summary>
        /// 创建配置log4net需要用到的XmlElement
        /// </summary>
        /// <returns></returns>
        public static XmlElement CreateConfigElement()
        {
            string path = Path.Combine(new string[] { AppDomain.CurrentDomain.BaseDirectory, "log4netConfig.xml" });
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                doc.Load(path);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"<?xml version=""1.0""?>");
                sb.Append(@"<log4net>");
                sb.Append(@"<appender name=""myAppender"" type=""log4net.Appender.RollingFileAppender,log4net"">");
                sb.Append(@"<param name=""File"" value=""" + Path.Combine(new string[]{ AppDomain.CurrentDomain.BaseDirectory,"Logs","Log"}) + @"""/>");
                sb.Append(@"<param name=""AppendToFile"" value=""true""/>");
                sb.Append(@"<param name=""RollingStyle"" value=""Composite""/>");
                sb.Append(@"<param name=""DatePattern"" value=""&quot;&quot;yyyy-MM-dd&quot;.log&quot;""/>");
                sb.Append(@"<param name=""maximumFileSize"" value=""2MB""/>");
                sb.Append(@"<param name=""maxSizeRollBackups"" value=""100""/>");
                sb.Append(@"<param name=""StaticLogFileName"" value=""false""/>");
                sb.Append(@"<layout type=""log4net.Layout.PatternLayout,log4net"">");
                sb.Append(@"<param name=""ConversionPattern"" value=""%n＝＝＝＝＝＝＝＝＝＝%n[Level]%-5level%n[Data]%date%n[Thread][%thread]%n[Run time][%r]ms%n[Line]%l%n[Message]%message""/>");
                sb.Append(@"<param name=""Header"" value=""&#xD;&#xA;----------------------启动--------------------------&#xD;&#xA;""/>");
                sb.Append(@"<param name=""Footer"" value=""&#xD;&#xA;----------------------结束--------------------------&#xD;&#xA;""/>");
                sb.Append(@"</layout>");
                sb.Append(@"<filter type=""log4net.Filter.LevelRangeFilter"">");
                sb.Append(@"<levelMin value=""INFO"" />");
                sb.Append(@"<levelMax value=""ERROR"" />");
                sb.Append(@"</filter>");
                sb.Append(@"</appender>");
                sb.Append(@"<!-- 所有日志 -->");
                sb.Append(@"<root>");
                sb.Append(@"<!-- 记录日志的日志级别： ALL, DEBUG, INFO, WARN, ERROR, FATAL, OFF -->");
                sb.Append(@"<priority value=""ALL""/>");
                sb.Append(@"<appender-ref ref=""myAppender""/>");
                sb.Append(@"</root>");
                sb.Append(@"</log4net>");
                doc.LoadXml(sb.ToString());
                doc.Save(path);
            }

            return doc.DocumentElement;
        }
    }
}
