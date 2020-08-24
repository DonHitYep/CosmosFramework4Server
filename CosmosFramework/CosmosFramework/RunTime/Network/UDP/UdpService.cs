﻿using System;
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
    public class UdpService : INetworkService, IControllable
    {
        protected UdpClient udpSocket;
        /// <summary>
        /// 对象IP
        /// </summary>
        protected string ip = "127.0.0.1";
        /// <summary>
        /// 对象端口
        /// </summary>
        protected int port = 20771;
        protected ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
        protected uint conv = 0;
        public bool IsPause { get; private set; }
        public virtual void OnInitialization()
        {
            udpSocket = new UdpClient(port);
            OnReceive();
        }
        /// <summary>
        /// 非空虚函数；
        /// 关闭这个服务；
        /// </summary>
        public virtual void OnTermination()
        {
            if (udpSocket != null)
            {
                udpSocket.Close();
                udpSocket = null;
            }
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
                    OnReceive();
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"网络消息接收异常：{e}");
                }
            }
        }
        /// <summary>
        /// 空虚函数;
        /// 发送报文信息
        /// </summary>
        /// <param name="conv">会话ID</param>
        /// <param name="data">数据报文</param>
        /// <param name="endPoint">远程对象</param>
        public virtual void SendMessage(INetworkMessage netMsg, IPEndPoint endPoint) { }
        /// <summary>
        /// 非空虚函数；
        /// 轮询更新;
        /// </summary>
        public virtual void OnRefresh() { }
        public void OnPause() { IsPause = true; }
        public void OnUnPause() { IsPause = false; }
    }
}
