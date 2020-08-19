using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Network
{
    public class UdpClientPeer:IRomotePeer
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public int SessionID { get; set; }
        public void Handler(INetworkMessage netMsg)
        {
        }
        public void OnInitialization()
        {
        }
        public void OnTermination()
        {
        }
    }
}
