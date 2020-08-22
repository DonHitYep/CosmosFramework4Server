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
            Utility.Debug.LogInfo("UdpServerService OnInitialization");
        }
        public override async void SendMessage(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            UdpClientPeer peer;
            if(clientDict.TryGetValue(netMsg.Conv,out peer))
            {
                UdpNetworkMessage udpNetMsg = netMsg as UdpNetworkMessage;
                var result= peer.EncodeMessage(ref udpNetMsg);
                if (result)
                {
                    if (udpSocket != null)
                    {
                        try
                        {
                            var buffer = udpNetMsg.GetBuffer();
                            int length = await udpSocket.SendAsync( buffer, buffer.Length, endPoint);
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
        public override void OnRefresh()
        {
            base.OnRefresh();
            pollingHandler?.Invoke(this);
            if (awaitHandle.Count > 0)
            {
                UdpReceiveResult data;
                if (awaitHandle.TryDequeue(out data))
                {
                    UdpNetworkMessage netMsg = ReferencePoolManager.Instance.Spawn<UdpNetworkMessage>();
                    if (netMsg.Conv == 0)
                    {
                        conv += 1;
                        netMsg.Conv = conv;
                        UdpClientPeer peer;
                        if (CreateClientPeer(netMsg,out peer))
                        {
                            peer.SetPeerEndPoint(data.RemoteEndPoint);
                        }
                    }
                    UdpClientPeer tmpPeer;
                    if(clientDict.TryGetValue(netMsg.Conv,out tmpPeer))
                    {
                        //如果peer失效，则移除
                        if (!tmpPeer.IsConnect)
                        {
                            pollingHandler -= tmpPeer.OnPolling;
                            UdpClientPeer abortPeer;
                            clientDict.TryRemove(netMsg.Conv, out abortPeer);
                            peerAbortHandler?.Invoke(abortPeer.Conv);
                        }
                        else
                        {
                            tmpPeer.MsgHandler(this,netMsg);
                        }
                    }
                }
            }
        }
        bool CreateClientPeer(UdpNetworkMessage udpNetMsg, out UdpClientPeer peer)
        {
            peer = default;
            bool result = false;
            if (!clientDict.TryGetValue(udpNetMsg.Conv, out peer))
            {
                peer = new UdpClientPeer(udpNetMsg.Conv);
                result = clientDict.TryAdd(udpNetMsg.Conv, peer);
                pollingHandler += peer.OnPolling;
            }
            return result;
        }
    }
}
