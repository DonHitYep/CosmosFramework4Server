using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Cosmos;
using Cosmos.Network;

namespace Cosmos
{
    public class ClientPeerManager : Module<ClientPeerManager>
    {

        public override void OnInitialization()
        {
            Utility.Debug.LogInfo("PeerManager OnInitialization");
            NetworkMsgEventCore.Instance.AddEventListener(10, PeerHandler);
            GameManager.NetworkManager.PeerConnectEvent += OnPeerConnectHandler;
            GameManager.NetworkManager.PeerDisconnectEvent+=OnPeerDisConnectHandler;
        }
        void PeerHandler(INetworkMessage netMsg)
        {
            UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
            Utility.Debug.LogWarning($"PeerManager接收到网络广播事件:{netMsg.ToString()}；消息体：{Utility.Converter.GetString(udpNetMsg.ServiceMsg)}");
            string str = "锟斤拷666";
            var data = Utility.Encode.ConvertToByte(str);
            UdpNetMessage msg = UdpNetMessage.EncodeMessage(udpNetMsg.Conv,10, data);
            GameManager.NetworkManager.SendNetworkMessage(msg);
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
