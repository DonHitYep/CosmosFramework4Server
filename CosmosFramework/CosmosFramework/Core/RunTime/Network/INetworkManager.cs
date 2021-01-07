using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Cosmos
{
    public interface INetworkManager : IModuleManager
    {
        IPEndPoint ServerEndPoint { get; }
        IPEndPoint ClientEndPoint { get; }

        event Action<IRemotePeer> PeerConnectEvent;
        event Action<IRemotePeer> PeerDisconnectEvent;

        void SendNetworkMessage(INetworkMessage netMsg);
        void SendNetworkMessage(byte[] buffer, IPEndPoint endPoint);
        void SendNetworkMessage(INetworkMessage netMsg, IPEndPoint endPoint);
        /// <summary>
        /// 初始化网络模块
        /// </summary>
        /// <param name="protocolType"></param>
        void Connect(ProtocolType protocolType);
        /// <summary>
        /// 与远程建立连接；
        /// 当前只有udp
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="protocolType">协议类型</param>
        void Connect(string ip, int port, ProtocolType protocolType);
        /// <summary>
        /// 与远程建立连接；
        /// </summary>
        /// <param name="service">自定义实现的服务</param>
        void Connect(INetworkService service);

        bool TryGetPeer(long key, out IRemotePeer value);
        bool ContainsPeer(long key);
        bool TryRemovePeer(long key);
        bool TryRemovePeer(long key, out IRemotePeer peer);
        bool TryAddPeer(long key, IRemotePeer value);
        bool TryUpdatePeer(long key, IRemotePeer newValue, IRemotePeer comparsionValue);
    }
}
