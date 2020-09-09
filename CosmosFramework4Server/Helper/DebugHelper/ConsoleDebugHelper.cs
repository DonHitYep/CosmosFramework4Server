using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace AscensionGateServer
{
    public class ConsoleDebugHelper : IDebugHelper
    {
        public void LogInfo(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"LogInfo : { msg}");
            Utility.Logger.Info(msg.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
        }
        /// <summary>
        /// log日志；
        /// 调用时msgColor参考((int)ConsoleColor.White).ToString(;
        /// </summary>
        /// <param name="msg">消息体</param>
        /// <param name="msgColor">消息颜色</param>
        /// <param name="context">内容，可传递对象</param>
        public void LogInfo(object msg, string msgColor, object context)
        {
            ConsoleColor color = (ConsoleColor)int.Parse(msgColor);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = color;
            Console.WriteLine($"INFO: { msg}");
            Utility.Logger.Info(msg.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
        }
        public void LogWarning(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARN : { msg}");
            Utility.Logger.Warring(msg.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n");
        }
        public void LogError(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR : { msg}");
            Utility.Logger.Error(null, msg.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
        }
        /// <summary>
        /// 会导致程序崩溃的log
        /// </summary>
        public void LogFatal(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"FATAL : { msg}");
            Utility.Logger.Warring(msg.ToString());
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n");
        }
    }
}
