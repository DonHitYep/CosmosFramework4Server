using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 服务端；
    /// 网络并发事件Core，Key为ushort码，表示OperationCode；值为INetworkMessage
    /// </summary>
    public class NetworkMsgEventCore:ConcurrentEventCore<ushort,INetworkMessage,NetworkMsgEventCore>
    {

    }
}
