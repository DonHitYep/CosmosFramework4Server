using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace CosmosFramework4Server
{
    public class PeerManager : Module<PeerManager>
    {
        public override void OnInitialization()
        {
            Utility.Debug.LogInfo("PeerManager OnInitialization");
            NetworkMsgEventCore.Instance.AddEventListener(10, PeerHandler);
            NetworkPeerEventCore.Instance.AddEventListener(NetworkOpCode._PeerConnect, OnPeerConnectHandler);
            NetworkPeerEventCore.Instance.AddEventListener(NetworkOpCode._PeerDisconnect, OnPeerDisConnectHandler);
        }
        void PeerHandler(INetworkMessage netMsg)
        {
            Utility.Debug.LogWarning($"PeerManager接收到网络广播事件:{netMsg.ToString()}；消息体：{Utility.Converter.GetString((netMsg as UdpNetMessage).ServiceMsg)}"); 
        }
        void OnPeerConnectHandler(IRemotePeer peer)
        {
            Utility.Debug.LogWarning($"Peer connect  连接 , conv :{ peer.Conv}");
        }
        void OnPeerDisConnectHandler(IRemotePeer peer)
        {
            Utility.Debug.LogWarning($"Peer disconnect 断线 , conv :{ peer.Conv}");
        }


    }
}
