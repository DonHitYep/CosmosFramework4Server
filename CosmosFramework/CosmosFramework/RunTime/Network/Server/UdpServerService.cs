using Cosmos.Network;
using Cosmos.Reference;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;

namespace Cosmos
{
    public class UdpServerService : UdpService
    {
        /// <summary>
        /// 轮询委托
        /// </summary>
        Action refreshHandler;
        /// <summary>
        /// 销毁一个peer事件处理者
        /// </summary>
        Action<uint> peerAbortHandler;
        ConcurrentDictionary<uint, UdpClientPeer> clientPeerDict = new ConcurrentDictionary<uint, UdpClientPeer>();
        public event Action RefreshHandler
        {
            add
            {
                refreshHandler += value;
            }
            remove
            {
                refreshHandler -= value;
            }
        }
        public override void OnInitialization()
        {
            base.OnInitialization();
        }
        public override async void SendMessage(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            UdpClientPeer peer;
            if (clientPeerDict.TryGetValue(netMsg.Conv, out peer))
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
        public override async void SendMessage(INetworkMessage netMsg)
        {
            UdpClientPeer peer;
            if (clientPeerDict.TryGetValue(netMsg.Conv, out peer))
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
            refreshHandler?.Invoke();
            if (awaitHandle.Count > 0)
            {
                UdpReceiveResult data;
                if (awaitHandle.TryDequeue(out data))
                {
                    UdpNetworkMessage netMsg = GameManager.ReferencePoolManager.Spawn<UdpNetworkMessage>();
                    netMsg.CacheDecodeBuffer(data.Buffer);
                    if (netMsg.Cmd == KcpProtocol.MSG)
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
                        if (clientPeerDict.TryGetValue(netMsg.Conv, out tmpPeer))
                        {
                            //如果peer失效，则移除
                            if (!tmpPeer.Available)
                            {
                                refreshHandler -= tmpPeer.OnRefresh;
                                UdpClientPeer abortedPeer;
                                clientPeerDict.TryRemove(netMsg.Conv, out abortedPeer);
                                peerAbortHandler?.Invoke(abortedPeer.Conv);
                                GameManager.ReferencePoolManager.Despawn(abortedPeer);
                                Utility.Debug.LogInfo($"移除失效的Peer，conv：{netMsg.Conv}:");
                            }
                            else
                            {
                                tmpPeer.MessageHandler(netMsg);
                            }
                        }
                        GameManager.ReferencePoolManager.Despawn(netMsg);
                    }
                }
            }
        }
        /// <summary>
        /// 移除失效peer
        /// </summary>
        /// <param name="conv">会话ID</param>
        public void AbortUnavilablePeer(uint conv)
        {
            try
            {
                UdpClientPeer tmpPeer;
                clientPeerDict.TryGetValue(conv, out tmpPeer);
                peerAbortHandler?.Invoke(conv);
                Utility.Debug.LogWarning($"心跳检测，移除失效peer , Conv :{ conv}");
            }
            catch (Exception e)
            {
                Utility.Debug.LogError($"心跳检测，移除失效peer失败", e);
            }
        }
        bool CreateClientPeer(UdpNetworkMessage udpNetMsg, IPEndPoint endPoint, out UdpClientPeer peer)
        {
            peer = default;
            bool result = false;
            if (!clientPeerDict.TryGetValue(udpNetMsg.Conv, out peer))
            {
                peer = GameManager.ReferencePoolManager.Spawn<UdpClientPeer>();
                peer.SetValue(SendMessage, udpNetMsg.Conv, endPoint);
                result = clientPeerDict.TryAdd(udpNetMsg.Conv, peer);
                refreshHandler += peer.OnRefresh;
                Utility.Debug.LogInfo($"Create ClientPeer  conv : {udpNetMsg.Conv}; PeerCount : {clientPeerDict.Count}");
            }
            return result;
        }
    }
}
