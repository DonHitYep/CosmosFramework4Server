using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 服务端；
    /// 网络并发Peer事件Core，Key为ushort码，表示 NetOpCode中的码；值为IRemotePeer
    /// </summary>
    internal class NetworkPeerEventCore:ConcurrentEventCore<ushort,IRemotePeer, NetworkPeerEventCore>
    {
    }
}
