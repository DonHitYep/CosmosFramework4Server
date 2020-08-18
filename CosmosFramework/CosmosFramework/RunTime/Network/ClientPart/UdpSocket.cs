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
namespace Cosmos.Network.Client
{
    /// <summary>
    /// UDP socket封装
    /// </summary>
    public class UdpSocket
    {
        UdpClient udpSocket;
        /// <summary>
        /// 远程对象
        /// </summary>
        IPEndPoint remote;
        /// <summary>
        /// 远程对象IP
        /// </summary>
        string remoteIP = "127.0.0.1";
        /// <summary>
        /// 远程对象端口
        /// </summary>
        int remotePort=20771;
        Action<INetworkMessage> dispatchNetMsgHandler;
        ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
        public UdpSocket(Action<INetworkMessage> dispatchNetMsgHandler)
        {
            this.dispatchNetMsgHandler = dispatchNetMsgHandler;
            remote = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
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
        public async void SendMessage(byte[] data,IPEndPoint iPEndPoint)
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
            //if()
        }
    }
}
