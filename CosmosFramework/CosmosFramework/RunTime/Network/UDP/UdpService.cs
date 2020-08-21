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
    public class UdpService : IRefreshable, IControllable
    {
        protected UdpClient udpSocket;
        /// <summary>
        /// IP对象；
        /// </summary>
        protected IPEndPoint endPoint;
        /// <summary>
        /// 对象IP
        /// </summary>
        protected string ip { get; set; }
        /// <summary>
        /// 对象端口
        /// </summary>
        protected int port = 20771;
        protected ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
        protected uint conv = 0;
        public bool IsPause { get; private set; }

        public UdpService()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            //构造传入0表示接收任意端口收发的数据
            udpSocket = new UdpClient(0);
        }
        /// <summary>
        /// 异步接收网络消息接口
        /// </summary>
        public virtual async void OnReceive()
        {
            if (udpSocket != null)
            {
                try
                {
                    UdpReceiveResult result = await udpSocket.ReceiveAsync();
                    awaitHandle.Enqueue(result);
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
        public virtual async void SendMessage(byte[] data, IPEndPoint endPoint)
        {
            if (udpSocket != null)
            {
                try
                {
                    int length = await udpSocket.SendAsync(data, data.Length, endPoint);
                    if (length == data.Length)
                    {
                        //长度相等表示完全发送
                    }
                    else
                    {
                        //若丢包，则重新发送
                        SendMessage(data, endPoint);
                    }
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"发送异常:{e.Message}");
                }
            }
        }
        public virtual void Close()
        {
            if (udpSocket != null)
            {
                udpSocket.Close();
                udpSocket = null;
            }
        }
        /// <summary>
        /// 轮询更新;
        /// </summary>
        public virtual void OnRefresh()
        {

        }

        public void OnPause()
        {
            IsPause = true;
        }
        public void OnUnPause()
        {
            IsPause = false;
        }
    }
}
