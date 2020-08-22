using System.Collections;
using Cosmos;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Network
{
    //TODO NetworkManager
    /// <summary>
    /// 此模块为客户端网络管理类
    /// </summary>
    public sealed class NetworkManager : ModuleBase<NetworkManager>,IRefreshable
    {
        string serverIP;
        int serverPort;
        string clientIP;
        int clientPort;
        INetworkService service;
        IPEndPoint serverEndPoint;
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
        public void OnRefresh()
        {
            service.OnRefresh();
        }
        public void SendNetworkMessage(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            service.SendMessage(netMsg, endPoint);
        }
        /// <summary>
        /// 初始化网络模块
        /// </summary>
        /// <param name="protocolType"></param>
        public void InitNetwork(ProtocolType protocolType)
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
                        PollingManager.Instance.AddPolling(service.OnRefresh);
                    }
                    break;
            }
        }
    }
}
