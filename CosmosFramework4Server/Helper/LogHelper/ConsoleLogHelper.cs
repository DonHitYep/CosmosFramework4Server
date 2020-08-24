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
            }
        }
        public void Error(Exception exception, string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > Error : Exception Message : {exception.Message} \n Exception line : {exception.StackTrace}; Msg : {msg}; \n {st}";
           Utility.IO.AppendWriteTextFile(logPath, logFileName, str);
        }
        public void Info(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > Info : {msg};\n{st}";
            Utility.IO.AppendWriteTextFile(logPath, logFileName, str);
        }
        public void Warring(string msg)
        {
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > Warring : {msg};\n {st}";
            Utility.IO.AppendWriteTextFile(logPath, logFileName, str);
        }
    }
}
