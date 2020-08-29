using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 框架预留操作码；
    /// 0~150预留给框架，剩下的码皆可自定义
    /// </summary>
    public  class NetOpCode
    {
        public static readonly ushort _PeerConnect= 1;
        public static readonly ushort _PeerDisconnect= 2;
        public static readonly ushort _Heartbeat= 101;
    }
}
