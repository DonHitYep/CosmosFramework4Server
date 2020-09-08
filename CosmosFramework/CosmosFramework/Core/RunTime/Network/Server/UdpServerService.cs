using Cosmos.Network;
using Cosmos.Reference;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Cosmos
{
    public class UdpServerService : UdpService
    {
        /// <summary>
        /// 轮询委托
        /// </summary>
        Action refreshHandler;
        public event Action RefreshHandler
        {
            add { refreshHandler += value; }
            remove { refreshHandler -= value; }
        }
        /// <summary>
        /// 销毁一个peer事件处理者
        /// </summary>
        Action<uint> peerAbortHandler;
        public event Action<uint> PeerAbortHandler
        {
            add { peerAbortHandler += value; }
            remove { peerAbortHandler -= value; }
        }
        ConcurrentDictionary<uint, UdpClientPeer> clientPeerDict = new ConcurrentDictionary<uint, UdpClientPeer>();
        public override void OnInitialization()
        {
            base.OnInitialization();
        }
        public override async void SendMessageAsync(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            UdpClientPeer peer;
            if (clientPeerDict.TryGetValue(netMsg.Conv, out peer))
            {
                UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
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
                                SendMessageAsync(udpNetMsg, endPoint);
                            }
                        }
                        catch (Exception e)
                        {
                            Utility.Debug.LogError($"Send net message exceotion :{e.Message}");
                        }
                    }
                }
            }
        }
        public override async void SendMessageAsync(INetworkMessage netMsg)
        {
            UdpClientPeer peer;
            if (clientPeerDict.TryGetValue(netMsg.Conv, out peer))
            {
                UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
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
                                SendMessageAsync(udpNetMsg);
                                Utility.Debug.LogInfo($"Send net KCP_ACK message");
                            }
                        }
                        catch (Exception e)
                        {
                            Utility.Debug.LogError($"Send net message exceotion : {e.Message}");
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
                    UdpNetMessage netMsg = GameManager.ReferencePoolManager.Spawn<UdpNetMessage>();
                    netMsg.CacheDecodeBuffer(data.Buffer);
                    if (netMsg.Cmd == KcpProtocol.MSG)
                        Utility.Debug.LogInfo($" OnRefresh KCP_MSG：{netMsg} ;ServiceMessage : {Utility.Converter.GetString(netMsg.ServiceMsg)},TS:{netMsg.TS}");
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
                                NetworkPeerEventCore.Instance.Dispatch(NetworkOpCode._PeerDisconnect, tmpPeer);
                                GameManager.ReferencePoolManager.Despawn(abortedPeer);
                                Utility.Debug.LogInfo($"Abort  Unavailable Peer，conv：{netMsg.Conv}:");
                            }
                            else
                            {
                                tmpPeer.MessageHandler(netMsg);
                            }
                        }
                        else
                        {
                            //发送终结命令；
                            UdpNetMessage finMsg = UdpNetMessage.DefaultMessageAsync(netMsg.Conv);
                            finMsg.Cmd = KcpProtocol.FIN;
                            SendFINMessageAsync(finMsg, data.RemoteEndPoint);
                        }
                        //GameManager.ReferencePoolManager.Despawn(netMsg);
                    }
                }
            }
        }
        async void SendFINMessageAsync(INetworkMessage netMsg, IPEndPoint endPoint)
        {

            UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
            udpNetMsg.TS = Utility.Time.MillisecondTimeStamp();
            udpNetMsg.EncodeMessage();
            if (udpSocket != null)
            {
                try
                {
                    var buffer = udpNetMsg.GetBuffer();
                    int length = await udpSocket.SendAsync(buffer, buffer.Length, endPoint);
                    if (length != buffer.Length)
                    {
                        //消息未完全发送，则重新发送
                        SendFINMessageAsync(udpNetMsg, endPoint);
                    }
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"Send net message exceotion:{e.Message}");
                }
            }
        }
        /// <summary>
        /// 移除失效peer；
        /// 作为参数传入peer；
        /// </summary>
        /// <param name="conv">会话ID</param>
        void AbortUnavilablePeer(uint conv)
        {
            try
            {
                UdpClientPeer tmpPeer;
                clientPeerDict.TryGetValue(conv, out tmpPeer);
                peerAbortHandler?.Invoke(conv);
                NetworkPeerEventCore.Instance.Dispatch(NetworkOpCode._PeerDisconnect, tmpPeer);
                Utility.Debug.LogWarning($" Conv :{ conv}  is Unavailable，remove peer ");
                GameManager.ReferencePoolManager.Despawn(tmpPeer);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError($"remove Unavailable peer fail {e}");
            }
        }
        bool CreateClientPeer(UdpNetMessage udpNetMsg, IPEndPoint endPoint, out UdpClientPeer peer)
        {
            peer = default;
            bool result = false;
            if (!clientPeerDict.TryGetValue(udpNetMsg.Conv, out peer))
            {
                peer = GameManager.ReferencePoolManager.Spawn<UdpClientPeer>();
                peer.SetValue(SendMessageAsync, AbortUnavilablePeer, udpNetMsg.Conv, endPoint);
                result = clientPeerDict.TryAdd(udpNetMsg.Conv, peer);
                refreshHandler += peer.OnRefresh;
                Utility.Debug.LogInfo($"Create ClientPeer  conv : {udpNetMsg.Conv}; PeerCount : {clientPeerDict.Count}");
                NetworkPeerEventCore.Instance.Dispatch(NetworkOpCode._PeerConnect, peer);
            }
            return result;
        }
    }
}
