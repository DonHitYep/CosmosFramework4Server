using System.Collections;
using System.Collections.Generic;
using System;
namespace Cosmos
{
    public sealed partial class Utility { 
        /// <summary>
        /// 时间相关工具，提供了不同精度的时间戳等函数
        /// </summary>
        public static class Time
        {
            //1s=1000ms
            //1ms=1000us
            readonly static long epoch = new DateTime(1970, 0, 0, 0, 0, 0, 0, DateTimeKind.Utc).Ticks;
            readonly static DateTime epochStruct = new DateTime(1970, 0, 0, 0, 0, 0, 0, DateTimeKind.Utc);
            /// <summary>
            /// 获取秒级别时间戳
            /// </summary>
            /// <returns>秒级别时间戳</returns>
            public static long SecondTimeStamp()
            {
                TimeSpan ts = DateTime.UtcNow - epochStruct;
                return Convert.ToInt64( ts.TotalSeconds);
            }
            /// <summary>
            /// 获取毫秒级别的时间戳
            /// </summary>
            /// <returns>毫秒级别时间戳</returns>
            public static long MillisecondTimeStamp()
            {
                TimeSpan ts = DateTime.UtcNow - epochStruct;
                return Convert.ToInt64(ts.TotalSeconds / 10000);
            }
            /// <summary>
            /// 获取微秒级别时间戳
            /// </summary>
            /// <returns>微秒级别时间戳</returns>
            public static long MicrosecondTimeStamp()
            {
                TimeSpan ts = DateTime.UtcNow - epochStruct;
                return Convert.ToInt64(ts.TotalSeconds);
            }
        }
    }
}
