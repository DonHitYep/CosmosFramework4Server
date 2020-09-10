using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 框架内部消息core
    /// </summary>
    internal class InnerEventCore:ConcurrentEventCore<byte,INetworkMessage,InnerEventCore>
    {
    }
}
