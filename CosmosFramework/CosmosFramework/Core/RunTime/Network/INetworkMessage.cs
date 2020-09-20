using System.Collections;
using System.Collections.Generic;
namespace Cosmos
{
    /// <summary>
    /// 网络消息接口
    /// </summary>
    public interface INetworkMessage
    {
        long Conv { get; }
        ushort OperationCode { get; }
        byte[] EncodeMessage();
        bool DecodeMessage(byte[] buffer);
        byte[] ServiceMsg { get; }
        byte[] GetBuffer();

    }
}