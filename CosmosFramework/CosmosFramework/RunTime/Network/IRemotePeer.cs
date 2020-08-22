using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// Peer对象接口；
    /// </summary>
    public interface IRemotePeer 
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        uint Conv { get; }
        //string LocalIP { get; }
        //int LocalPort { get; }
        //IPAddress LocalIPAddress { get; }
        //string RemoteIP { get; }
        //int RemotePort { get; }
        //IPAddress RemoteIPAddress { get; }
        ///// <summary>
        ///// 处理网络消息；
        ///// </summary>
        ///// <param name="netMsg">网络消息</param>
        //void MsgHandler(INetworkMessage netMsg);
        //void AbortConnection();
        //void Disconnect();
        //void Dispose();
        //void Flush();
    }
}
