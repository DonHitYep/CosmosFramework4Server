using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Cosmos;
namespace Cosmos.Network
{
    /// <summary>
    /// UDP socket服务；
    /// 这里管理其他接入的远程对象；
    /// </summary>
    public class UdpService
    {
        UdpClient udpSocket;
        /// <summary>
        /// 远程对象
        /// </summary>
        IPEndPoint remoteEndPoint;
        /// <summary>
        /// 远程对象IP
        /// </summary>
        string remoteIP = "127.0.0.1";
        /// <summary>
        /// 远程对象端口
        /// </summary>
        int remotePort = 20771;
        Action<INetworkMessage> dispatchNetMsgHandler;
        ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
        CancellationTokenSource cancelToken = new CancellationTokenSource();
        ConcurrentDictionary<int, UClient> clients = new ConcurrentDictionary<int, UClient>();

        public UdpService(Action<INetworkMessage> dispatchNetMsgHandler)
        {
            this.dispatchNetMsgHandler = dispatchNetMsgHandler;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            udpSocket = new UdpClient(remotePort);
        }
        /// <summary>
        /// 异步接收网络消息接口
        /// </summary>
        public async void OnReceive()
        {
            if (udpSocket != null)
            {
                try
                {
                    UdpReceiveResult result = await udpSocket.ReceiveAsync();
                    awaitHandle.Enqueue(result);
                    OnReceive();
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"网络消息接收异常：{e}");
                }
            }
        }
        public async void SendMessage(byte[] data, IPEndPoint iPEndPoint)
        {
            if (udpSocket != null)
            {
                try
                {
                    int length = await udpSocket.SendAsync(data, data.Length, iPEndPoint);
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"发送异常:{e.Message}");
                }
            }
        }
        public void Close()
        {
            cancelToken.Cancel();
            foreach (var client in clients.Values)
            {
                client.OnTermination();
            }
            clients.Clear();
            if (udpSocket != null)
            {
                udpSocket.Close();
                udpSocket = null;
            }
            dispatchNetMsgHandler = null;
        }
        int sessionID = 0;
        async Task Handle()
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (awaitHandle.Count>0)
                {
                    UdpReceiveResult data;
                    if(awaitHandle.TryDequeue(out data))
                    {
                        UdpNetworkMessage netMsg = new UdpNetworkMessage(data.Buffer);
                        if (netMsg.IsFull)
                        {
                            if (netMsg.SessionID == 0)
                            {
                                sessionID += 1;
                                netMsg.SessionID = sessionID;
                                CreateUClient(netMsg);
                            }
                            UClient targetClient;
                            if(clients.TryGetValue(netMsg.SessionID,out targetClient))
                            {
                                targetClient.Handler(netMsg);
                            }
                        }
                    }
                }
            }
        }
        void CreateUClient(INetworkMessage netMsg)
        {
        }
    }
}
