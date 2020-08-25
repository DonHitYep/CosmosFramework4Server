using Cosmos.Network;
using Cosmos.Reference;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Cosmos
{
    public class UdpServerService : UdpService
    {
        /// <summary>
        /// 销毁一个peer事件处理者
        /// </summary>
        Action<uint> peerAbortHandler;
        ConcurrentDictionary<uint, UdpClientPeer> clientDict = new ConcurrentDictionary<uint, UdpClientPeer>();
        /// <summary>
        /// 轮询委托
        /// </summary>
        Action<UdpServerService> pollingHandler;
        public override void OnInitialization()
        {
            base.OnInitialization();
        }
        public override async void SendMessage(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            UdpClientPeer peer;
            if (clientDict.TryGetValue(netMsg.Conv, out peer))
            {
                UdpNetworkMessage udpNetMsg = netMsg as UdpNetworkMessage;
                var result = peer.EncodeMessage(ref udpNetMsg);
                if (result)
                {
                    if (udpSocket != null)
                    {
                        try
                        {
                            var buffer = udpNetMsg.GetBuffer();
                            int length = await udpSocket.SendAsync(buffer, buffer.Length, endPoint);
                            if (length != buffer.Length)
                            {
                                //消息未完全发送，则重新发送
                                SendMessage(udpNetMsg, endPoint);
                            }
                        }
                        catch (Exception e)
                        {
                            Utility.Debug.LogError($"发送异常:{e.Message}");
                        }
                    }
                }
            }
        }
        public override  async void SendMessage(INetworkMessage netMsg)
        {
            UdpClientPeer peer;
            if (clientDict.TryGetValue(netMsg.Conv, out peer))
            {
                UdpNetworkMessage udpNetMsg = netMsg as UdpNetworkMessage;
                var result = peer.EncodeMessage(ref udpNetMsg);
                if (result)
                {
                    if (udpSocket != null)
                    {
                        try
                        {
                            var buffer = udpNetMsg.GetBuffer();
                            int length = await udpSocket.SendAsync(buffer, buffer.Length, peer.PeerEndPoint);
                            if (length != buffer.Length)
                            {
                                //消息未完全发送，则重新发送
                                SendMessage(udpNetMsg);
                            }
                        }
                        catch (Exception e)
                        {
                            Utility.Debug.LogError($"发送异常:{e.Message}");
                        }
                    }
                }
            }
        }
        public override void OnRefresh()
        {
            pollingHandler?.Invoke(this);
            if (awaitHandle.Count > 0)
            {
                UdpReceiveResult data;
                if (awaitHandle.TryDequeue(out data))
                {
                    UdpNetworkMessage netMsg = ReferencePoolManager.Instance.Spawn<UdpNetworkMessage>();
                    netMsg.CacheDecodeBuffer(data.Buffer);
                    Utility.Debug.LogInfo($" 解码从客户端接收的报文：{netMsg.ToString()} ;ServiceMessage : {Utility.Converter.GetString(netMsg.ServiceMsg)}");
                    if (netMsg.IsFull)
                    {
                        if (netMsg.Conv == 0)
                        {
                            conv += 1;
                            netMsg.Conv = conv;
                            UdpClientPeer peer;
                            CreateClientPeer(netMsg, data.RemoteEndPoint, out peer);
                        }
                        UdpClientPeer tmpPeer;
                        if (clientDict.TryGetValue(netMsg.Conv, out tmpPeer))
                        {
                            //如果peer失效，则移除
                            if (!tmpPeer.IsConnect)
                            {
                                pollingHandler -= tmpPeer.OnPolling;
                                UdpClientPeer abortPeer;
                                clientDict.TryRemove(netMsg.Conv, out abortPeer);
                                peerAbortHandler?.Invoke(abortPeer.Conv);
                                Utility.Debug.LogInfo($"移除失效peer，conv{netMsg.Conv}:");
                            }
                            else
                            {
                                tmpPeer.MessageHandler(this, netMsg);
                            }
                        }
                        ReferencePoolManager.Instance.Despawn(netMsg);
                    }
                }
            }
        }
        bool CreateClientPeer(UdpNetworkMessage udpNetMsg, IPEndPoint endPoint, out UdpClientPeer peer)
        {
            peer = default;
            bool result = false;
            if (!clientDict.TryGetValue(udpNetMsg.Conv, out peer))
            {
                peer = new UdpClientPeer(udpNetMsg.Conv);
                peer.SetPeerEndPoint(endPoint);
                result = clientDict.TryAdd(udpNetMsg.Conv, peer);
                pollingHandler += peer.OnPolling;
                Utility.Debug.LogInfo($"CreateClientPeer  conv : {udpNetMsg.Conv}; PeerCount : {clientDict.Count}");
            }
            return result;
        }
    }
}
