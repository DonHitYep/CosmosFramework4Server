using System;
using System.Collections;
using System.Collections.Generic;
namespace Cosmos
{
    /// <summary>
    /// 异常处理模块
    /// </summary>
    internal class ExceptionManager 
    {
        /// <summary>
        /// 是否输出日志
        /// </summary>
        public bool ExportLog { get; set; }
        //TODO 全局异常处理模块，调用C# 与 Unity特性实现
        /// <summary>
        /// 抛出异常
        /// </summary>
        /// <param name="e">异常对象</param>
        internal void ThrowException( Exception e,string logString)
        {

        }
    }
}