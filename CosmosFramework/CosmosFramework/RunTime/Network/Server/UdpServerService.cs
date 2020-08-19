using Cosmos.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public class UdpServerService : UdpService
    {
        public UdpServerService(Action<INetworkMessage> dispatchNetMsgHandler) : base(dispatchNetMsgHandler)
        {
        }
    }
}
