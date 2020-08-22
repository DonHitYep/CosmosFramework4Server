using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace ProtocolCore
{
    public class ConsoleDebugHelper : IDebugHelper
    {
        public void LogError(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"LogError : { msg}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
        }
        public void LogInfo(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"LogInfo : { msg}");
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
            ConsoleColor color =(ConsoleColor) int.Parse(msgColor);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = color;
            Console.WriteLine($"LogInfo : { msg}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<\n");
        }
        public void LogWarning(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"LogWarring : { msg}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>\n");
        }
    }
}
