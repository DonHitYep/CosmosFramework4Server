using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Cosmos;
using Cosmos.Log;

namespace ProtocolCore
{
    public class ConsoleLogHelper : ILogHelper
    {
        string logPath;
        string logFileName="CosmosServerLog.log";
        public ConsoleLogHelper()
        {
            if (logPath == null)
            {
                DirectoryInfo info = Directory.GetParent(Environment.CurrentDirectory);
                string str = info.Parent.Parent.Parent.FullName;
                logPath = Utility.IO.CombineRelativePath(str, "ServerLog");
                Utility.IO.CreateFolder(logPath);
                System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            }
        }
        public void Error(Exception exception, string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > ERROR : Exception Message : {exception?.Message} ；Exception line : {exception?.StackTrace}; Msg : {msg};\nStackTrace[ - ] ：{st}";
           Utility.IO.AppendWriteTextFile(logPath, logFileName, str);
        }
        public void Info(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > INFO : {msg};\nStackTrace[ - ] ：{st}";
            Utility.IO.AppendWriteTextFile(logPath, logFileName, str);
        }
        public void Warring(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > WARN : {msg};\nStackTrace[ - ] ：{st}";
            Utility.IO.AppendWriteTextFile(logPath, logFileName, str);
        }
        public void Fatal(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > FATAL : {msg};\nStackTrace[ - ] ：{st}";
            Utility.IO.AppendWriteTextFile(logPath, logFileName, str);
        }
        /// <summary>
        /// 全局异常捕获器
        /// </summary>
        /// <param name="sender">异常抛出者</param>
        /// <param name="e">未被捕获的异常</param>
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Utility.Debug.LogError(e);
        }
    }
}
