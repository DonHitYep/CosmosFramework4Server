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
        Action<long> peerAbortHandler;
        public event Action<long> PeerAbortHandler
        {
            add { peerAbortHandler += value; }
            remove { peerAbortHandler -= value; }
        }
        //ConcurrentDictionary<uint, UdpClientPeer> clientPeerDict = new ConcurrentDictionary<uint, UdpClientPeer>();
        public override void OnInitialization()
        {
            base.OnInitialization();
        }
        public override async void SendMessageAsync(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            IRemotePeer tmpPeer;
            if (GameManager.NetworkManager.TryGetValue(netMsg.Conv, out tmpPeer))
            {
                UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
                UdpClientPeer peer = tmpPeer as UdpClientPeer;
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
            IRemotePeer tmpPeer;
            if (GameManager.NetworkManager.TryGetValue(netMsg.Conv, out tmpPeer))
            {
                UdpClientPeer peer = tmpPeer as UdpClientPeer;
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
                    netMsg.DecodeMessage(data.Buffer);
#if DEBUG
                    if (netMsg.Cmd == KcpProtocol.MSG)
                        Utility.Debug.LogInfo($" OnRefresh KCP_MSG：{netMsg} ;ServiceMessage : {Utility.Converter.GetString(netMsg.ServiceMsg)},TS:{netMsg.TS}");
#endif
                    if (netMsg.IsFull)
                    {
                        if (netMsg.Conv == 0)
                        {
                            conv += 1;
                            netMsg.Conv = conv;
                            UdpClientPeer peer;
                            CreateClientPeer(netMsg, data.RemoteEndPoint, out peer);
                        }
                        IRemotePeer rPeer;
                        if (GameManager.NetworkManager .TryGetValue(netMsg.Conv, out rPeer))
                        {
                            UdpClientPeer tmpPeer = rPeer as UdpClientPeer;
                            //如果peer失效，则移除
                            if (!tmpPeer.Available)
                            {
                                refreshHandler -= tmpPeer.OnRefresh;
                                IRemotePeer abortedPeer;
                                GameManager.NetworkManager.TryRemove(netMsg.Conv,out abortedPeer);
                                //UdpClientPeer abortedUdpPeer = abortedPeer as UdpClientPeer;
                                peerAbortHandler?.Invoke(abortedPeer.Conv);
                                NetworkPeerEventCore.Instance.Dispatch(InnerOpCode._PeerDisconnect, tmpPeer);
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
                            UdpNetMessage finMsg = UdpNetMessage.EncodeMessage(netMsg.Conv);
                            finMsg.Cmd = KcpProtocol.FIN;
                            SendFINMessageAsync(finMsg, data.RemoteEndPoint);
                        }
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
        void AbortUnavilablePeer(long conv)
        {
            try
            {
                IRemotePeer tmpPeer;
                GameManager.NetworkManager.TryGetValue(conv, out tmpPeer);
                peerAbortHandler?.Invoke(conv);
                NetworkPeerEventCore.Instance.Dispatch(InnerOpCode._PeerDisconnect, tmpPeer);
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
            if (!GameManager.NetworkManager.ContainsKey(udpNetMsg.Conv))
            {
                peer = GameManager.ReferencePoolManager.Spawn<UdpClientPeer>();
                peer.SetValue(SendMessageAsync, AbortUnavilablePeer, udpNetMsg.Conv, endPoint);
                result = GameManager.NetworkManager.TryAdd(udpNetMsg.Conv, peer);
                refreshHandler += peer.OnRefresh;
                NetworkPeerEventCore.Instance.Dispatch(InnerOpCode._PeerConnect, peer);
            }
            return result;
        }
    }
}
