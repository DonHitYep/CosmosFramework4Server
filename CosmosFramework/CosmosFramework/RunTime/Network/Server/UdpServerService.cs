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
        ConcurrentDictionary<uint, UdpClientPeer> clientDict = new ConcurrentDictionary<uint, UdpClientPeer>();
        HashSet<uint> ackSNSet = new HashSet<uint>();
         bool CreateClientPeer(UdpNetworkMessage udpNetMsg,out UdpClientPeer peer)
        {
            peer =default;
            bool result=false;
            if(!clientDict.TryGetValue(udpNetMsg.Conv,out peer))
            {
                peer = new UdpClientPeer(udpNetMsg.Conv);
                result= clientDict.TryAdd(udpNetMsg.Conv, peer);
            }
            return result;
        }
        public override void OnRefresh()
        {
            base.OnRefresh();
            OnReceive();
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
                        tmpPeer.MsgHandler(netMsg);
                    }
                }
            }
        }
    }
}
