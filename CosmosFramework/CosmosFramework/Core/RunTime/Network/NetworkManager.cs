using System.Collections;
using Cosmos;
using System.Net;
using System.Net.Sockets;
using System;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Cosmos.Network
{
    //TODO NetworkManager
    [Module]
    /// <summary>
    /// 此模块为客户端网络管理类
    /// </summary>
    internal sealed partial class NetworkManager : Module, INetworkManager
    {
        string serverIP;
        int serverPort;
        string clientIP;
        int clientPort;
        INetworkService service;
        IPEndPoint serverEndPoint;
        INetMessageHelper netMessageHelper;
        public IPEndPoint ServerEndPoint
        {
            get
            {
                if (serverEndPoint == null)
                    serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                return serverEndPoint;
            }
        }
        IPEndPoint clientEndPoint;
        public IPEndPoint ClientEndPoint
        {
            get
            {
                if (clientEndPoint == null)
                    clientEndPoint = new IPEndPoint(IPAddress.Parse(clientIP), clientPort);
                return clientEndPoint;
            }
        }


        public int PeerCount { get { return clientPeerDict.Count; } }
        ConcurrentDictionary<long, IRemotePeer> clientPeerDict = new ConcurrentDictionary<long, IRemotePeer>();
        Action<IRemotePeer> peerConnectHandler;
        public event Action<IRemotePeer> PeerConnectEvent
        {
            add { peerConnectHandler += value; }
            remove { peerConnectHandler -= value; }
        }
        Action<IRemotePeer> peerDisconnectHandler;
        public event Action<IRemotePeer> PeerDisconnectEvent
        {
            add { peerDisconnectHandler += value; }
            remove { peerDisconnectHandler -= value; }
        }

        public override void OnInitialization()
        {
            IsPause = false;
        }
        public override void OnRefresh()
        {
            service?.OnRefresh();
        }
        /// <summary>
        /// 本质是异步发送
        /// </summary>
        /// <param name="netMsg">消息对象</param>
        public void SendNetworkMessage(INetworkMessage netMsg)
        {
            service.SendMessageAsync(netMsg);
        }
        public void SendNetworkMessage(byte[] buffer, IPEndPoint endPoint)
        {
            service.SendMessageAsync(buffer, endPoint);
        }
        public void SendNetworkMessage(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            service.SendMessageAsync(netMsg, endPoint);
        }
        /// <summary>
        /// 初始化网络模块
        /// </summary>
        /// <param name="protocolType"></param>
        public void Connect(ProtocolType protocolType)
        {
            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    {
                    }
                    break;
                case ProtocolType.Udp:
                    {
                        service = new UdpServerService();
                        service.OnInitialization();
                    }
                    break;
            }
        }
        /// <summary>
        /// 与远程建立连接；
        /// 当前只有udp
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="protocolType">协议类型</param>
        public void Connect(string ip, int port, ProtocolType protocolType)
        {
            OnUnPause();
            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    {
                    }
                    break;
                case ProtocolType.Udp:
                    {
                        service = new UdpServerService();
                        UdpServerService udp = service as UdpServerService;
                        udp.IP = ip;
                        udp.Port = port;
                        service.OnInitialization();
                    }
                    break;
            }
        }
        /// <summary>
        /// 与远程建立连接；
        /// </summary>
        /// <param name="service">自定义实现的服务</param>
        public void Connect(INetworkService service)
        {
            if (service == null)
            {
                Utility.Debug.LogError("Service Empty");
                return;
            }
            OnUnPause();
            this.service = service;
            service.OnInitialization();
            Utility.Debug.LogInfo("建立UDP远程连接");
        }

        public bool TryGetPeer(long key, out IRemotePeer value)
        {
            return clientPeerDict.TryGetValue(key, out value);
        }
        public bool ContainsPeer(long key)
        {
            return clientPeerDict.ContainsKey(key);
        }
        public bool TryRemovePeer(long key)
        {
            IRemotePeer peer;
            var result = clientPeerDict.TryRemove(key, out peer);
            if (result)
                peerDisconnectHandler?.Invoke(peer);
            return result;
        }
        public bool TryRemovePeer(long key, out IRemotePeer peer)
        {
            var result = clientPeerDict.TryRemove(key, out peer);
            if (result)
                peerDisconnectHandler?.Invoke(peer);
            return result;
        }
        public bool TryAddPeer(long key, IRemotePeer value)
        {
            var result = clientPeerDict.TryAdd(key, value);
            if (result)
                peerConnectHandler?.Invoke(value);
            return result;
        }
        public bool TryUpdatePeer(long key, IRemotePeer newValue, IRemotePeer comparsionValue)
        {
            var result = clientPeerDict.TryUpdate(key, newValue, comparsionValue);
            if (result)
            {
                peerConnectHandler?.Invoke(newValue);
                peerDisconnectHandler?.Invoke(comparsionValue);
            }
            return result;
        }
    }
}
