using System.Collections;
using Cosmos;
using System.Net;
using System.Net.Sockets;
using System;
using System.Diagnostics;

namespace Cosmos.Network
{
    //TODO NetworkManager
    /// <summary>
    /// 此模块为客户端网络管理类
    /// </summary>
    public sealed partial class NetworkManager : Module<NetworkManager>
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
        public override void OnInitialization()
        {
            IsPause = false;
        }
        public override void OnRefresh()
        {
            service?.OnRefresh();
        }
        public void SendNetworkMessage(INetworkMessage netMsg)
        {
            service.SendMessageAsync(netMsg);
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
    }
}
