using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Cosmos;

namespace ProtocolCore
{
    public class ConsoleLoggerHelper : ILoggerHelper
    {
        string logFullPath;
        string logFileName = "CosmosFrameworkServer.log";
        string logFolderName = "Log";
        /// <summary>
        /// 默认构造，使用默认地址与默认log名称
        /// </summary>
        public ConsoleLoggerHelper()
        {
            DirectoryInfo info = Directory.GetParent(Environment.CurrentDirectory);
            string str = info.Parent.Parent.Parent.FullName;
            logFullPath = Utility.IO.CombineRelativePath(str, logFolderName);
            Utility.IO.CreateFolder(logFullPath);
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
        }
        public void Error(Exception exception, string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > ERROR : Exception Message : {exception?.Message} ；Exception line : {exception?.StackTrace}; Msg : {msg};\nStackTrace[ - ] ：{st}";
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
        }
        public void Info(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > INFO : {msg};\nStackTrace[ - ] ：{st}";
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
        }
        public void Warring(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > WARN : {msg};\nStackTrace[ - ] ：{st}";
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
        }
        public void Fatal(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > FATAL : {msg};\nStackTrace[ - ] ：{st}";
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
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
