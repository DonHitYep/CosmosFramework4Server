using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Cosmos;
using Cosmos.Network;

namespace CosmosServer
{
    [Module]
    public class ClientPeerManager : Module,IClientPeerManager
    {
        INetworkManager networkManager;
        public override void OnInitialization()
        {
            NetworkMsgEventCore.Instance.AddEventListener(10, PeerHandler);
        }
        public override void OnPreparatory()
        {
            networkManager = GameManager.GetModule<INetworkManager>();
            networkManager.PeerConnectEvent += OnPeerConnectHandler;
            networkManager.PeerDisconnectEvent += OnPeerDisConnectHandler;
        }
        void PeerHandler(INetworkMessage netMsg)
        {
            UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
            Utility.Debug.LogWarning($"ClientPeerManager接收到网络广播事件:{netMsg}；消息体：{Utility.Converter.GetString(udpNetMsg.ServiceMsg)}");
            string str = "锟斤拷666";
            var data = Utility.Encode.ConvertToByte(str);
            UdpNetMessage msg = UdpNetMessage.EncodeMessage(udpNetMsg.Conv,10, data);
            networkManager.SendNetworkMessage(msg);
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
