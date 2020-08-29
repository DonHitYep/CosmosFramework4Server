using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Cosmos
{
    public class KcpChannel : NetChannel
    {
        public override byte[] EncodingMessage(INetMessage message)
        {
            return null;
        }
        public override INetMessage ReceiveMessage(Socket client)
        {
            return null;
        }
        public override void OnRefresh()
        {
            base.OnRefresh();
        }
        protected override void ReceiveMessage()
        {
        }
        protected override void SendMessage()
        {
        }
    }
}
