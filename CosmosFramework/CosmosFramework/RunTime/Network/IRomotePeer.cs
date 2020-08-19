using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// Peer对象接口；
    /// </summary>
    public interface IRomotePeer : IBehaviour
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        int SessionID { get; set; }
        /// <summary>
        /// 空虚函数；
        /// 处理网络消息；
        /// </summary>
        /// <param name="netMsg"></param>
        void Handler(INetworkMessage netMsg);
    }
}
