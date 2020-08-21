using Cosmos.Network;
using Cosmos.Reference;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
        HashSet<uint> ackSNSet = new HashSet<uint>();
        Action refreshHandler;
         bool CreateClientPeer(UdpNetworkMessage udpNetMsg,out UdpClientPeer peer)
        {
            peer =default;
            bool result=false;
            if(!clientDict.TryGetValue(udpNetMsg.Conv,out peer))
            {
                peer = new UdpClientPeer(udpNetMsg.Conv);
                result= clientDict.TryAdd(udpNetMsg.Conv, peer);
                refreshHandler+= peer.OnRefresh;
            }
            return result;
        }
        public override void OnRefresh()
        {
            base.OnRefresh();
            OnReceive();
            refreshHandler?.Invoke();
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
                        if (tmpPeer.IsAbort)
                        {
                            refreshHandler -= tmpPeer.OnRefresh;
                            UdpClientPeer abortPeer;
                            clientDict.TryRemove(netMsg.Conv, out abortPeer);
                            peerAbortHandler?.Invoke(abortPeer.Conv);
                        }
                        else
                        {
                            tmpPeer.MsgHandler(netMsg);
                        }
                    }
                }
            }
        }
    }
}
