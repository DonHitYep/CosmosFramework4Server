using System.Collections;
using Cosmos;
using System.Net;
namespace Cosmos.Network
{
    //TODO NetworkManager
    /// <summary>
    /// 此模块为客户端网络管理类
    /// </summary>
    internal sealed class NetworkManager 
    {
        string serverIP;
        int serverPort;
        string clientIP;
        int clientPort;
        IPEndPoint serverEndPoint;
        internal IPEndPoint ServerEndPoint
        {
            get
            {
                if (serverEndPoint == null)
                    serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                return serverEndPoint;
            }
        }
        IPEndPoint clientEndPoint;
        internal IPEndPoint ClientEndPoint
        {
            get
            {
                if (clientEndPoint == null)
                    clientEndPoint = new IPEndPoint(IPAddress.Parse(clientIP), clientPort);
                return clientEndPoint;
            }
        }
        /// <summary>
        /// 建立远程连接
        /// </summary>
        internal void Connect()
        {

        }
    }
}
