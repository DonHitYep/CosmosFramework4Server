using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    public class TcpNetworkMessage : INetworkMessage
    {
        public void DecodeMessage(byte[] buffer)
        {
        }
        public byte[] EncodeMessage()
        {
            return null;
        }
    }
}
