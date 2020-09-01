using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    public class UdpChannel : NetChannel
    {
        public override ProtocolType Protocol { get { return ProtocolType.Udp; } }
        public override SocketType SocketType { get { return SocketType.Dgram; } }
        public override bool IsNeedConnect { get { return false; } }
        public override byte[] EncodingMessage(INetMessage message)
        {
            UdpNetMessage udpNetMsg = message as UdpNetMessage;
            return udpNetMsg.EncodeMessage();
        }
        public override INetMessage ReceiveMessage(Socket client)
        {
            try
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override void SendMessage()
        {
        }

        protected override void ReceiveMessage()
        {
        }
    }
}
