using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    public class TcpChannel : NetChannel
    {
        public override ProtocolType Protocol { get { return ProtocolType.Tcp; } }
        public override SocketType SocketType { get { return SocketType.Stream; } }
        public override bool IsNeedConnect { get { return true; } }
        public override INetMessage ReceiveMessage(Socket client)
        {
            throw new NotImplementedException();
        }
        public override byte[] EncodingMessage(INetMessage message)
        {
            throw new NotImplementedException();
        }
        protected override void SendMessage()
        {
            throw new NotImplementedException();
        }
        protected override void ReceiveMessage()
        {
            throw new NotImplementedException();
        }
    }
}
