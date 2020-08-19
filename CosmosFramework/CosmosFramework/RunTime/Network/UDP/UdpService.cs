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
using Cosmos.Reference;

namespace Cosmos.Network
{
    /// <summary>
    /// UDP socket服务；
    /// 这里管理其他接入的远程对象；
    /// </summary>
    public class UdpService : IRefreshable
    {
        UdpClient udpSocket;
        /// <summary>
        /// 远程对象；
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
        ConcurrentDictionary<int, UdpClientPeer> clients = new ConcurrentDictionary<int, UdpClientPeer>();
        int sessionID = 0;

        public UdpService(Action<INetworkMessage> dispatchNetMsgHandler)
        {
            this.dispatchNetMsgHandler = dispatchNetMsgHandler;
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            //构造传入0表示接收任意端口收发的数据
            udpSocket = new UdpClient(0);
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
        /// <summary>
        /// 发送报文信息
        /// </summary>
        /// <param name="data">报文数据</param>
        public async void SendMessage(byte[] data)
        {
            if (udpSocket != null)
            {
                try
                {
                    int length = await udpSocket.SendAsync(data, data.Length, remoteEndPoint);
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"发送异常:{e.Message}");
                }
            }
        }
        public void Close()
        {
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
        /// <summary>
        /// 轮询更新;
        /// 客户端建议在FixUpdate中使用；
        /// 
        /// </summary>
        public void OnRefresh()
        {
            if (awaitHandle.Count > 0)
            {
                UdpReceiveResult data;
                if (awaitHandle.TryDequeue(out data))
                {
                    UdpNetworkMessage netMsg = ReferencePoolManager.Instance.Spawn<UdpNetworkMessage>();
                    netMsg.DecodeMessage(data.Buffer);
                }
            }
        }
    }
}
