using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Cosmos.Network
{
    public class UdpClientPeer : IRemotePeer, IRefreshable
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public long Conv { get; private set; }
        /// <summary>
        /// 分配的endPoint
        /// </summary>
        public IPEndPoint PeerEndPoint { get; private set; }
        /// <summary>
        /// 处理的message序号，按1累次递增。
        /// </summary>
        public uint HandleSN { get; set; }
        /// <summary>
        /// 已经发送的消息序号
        /// </summary>
        public uint SendSN { get; set; }
        /// <summary>
        /// 当前Peer是否处于连接状态
        /// </summary>
        public bool Available { get; private set; }
        /// <summary>
        /// 心跳机制
        /// </summary>
        public IHeartbeat Heartbeat { get; private set; }
        /// <summary>
        /// 最后一次更新时间；
        /// 更新的时间戳；
        /// </summary>
        protected long latestPollingTime;
        /// <summary>
        /// 并发发送消息的字典；
        /// 整理错序报文；
        /// 临时起到ACK缓存的作用
        /// </summary>
        protected ConcurrentDictionary<uint, UdpNetMessage> sndMsgDict;
        /// <summary>
        /// 收到的待处理的错序报文
        /// </summary>
        protected ConcurrentDictionary<uint, UdpNetMessage> rcvMsgDict;
        /// <summary>
        /// 解析间隔
        /// </summary>
        protected const int interval = 2000;
        /// <summary>
        /// 发送网络消息委托；
        /// 这里函数指针指向service的sendMessage
        /// </summary>
        Action<INetworkMessage> sendMessageHandler;
        Action<long> abortPeerHandler;
        public UdpClientPeer()
        {
            sndMsgDict = new ConcurrentDictionary<uint, UdpNetMessage>();
            rcvMsgDict = new ConcurrentDictionary<uint, UdpNetMessage>();
            //TODO Heartbeat 需要能够自定义传入，可扩展；
            Heartbeat = new Heartbeat();
        }
        public UdpClientPeer(uint conv) : this()
        {
            this.Conv = conv;
        }
        /// <summary>
        /// 空虚函数
        /// 发送消息给这个peer的远程对象
        /// </summary>
        /// <param name="netMsg">消息体</param>
        public virtual void SendMessage(INetworkMessage netMsg)
        {
            sendMessageHandler?.Invoke(netMsg);
        }
        public void SetValue(Action<INetworkMessage> sendMsgCallback, Action<long> abortPeerCallback, long conv, IPEndPoint endPoint)
        {
            this.Conv = conv;
            this.PeerEndPoint = endPoint;
            latestPollingTime = Utility.Time.MillisecondNow() + interval;
            this.sendMessageHandler += sendMsgCallback;
            this.abortPeerHandler = abortPeerCallback;
            Heartbeat.Conv = conv;
            Heartbeat.OnActive();
            Heartbeat.UnavailableHandler = AbortConnection;
            Available = true;
        }
        /// <summary>
        /// 处理进入这个peer的消息
        /// </summary>
        /// <param name="service">udp服务</param>
        /// <param name="msg">消息体</param>
        public virtual void MessageHandler(INetworkMessage msg)
        {
            UdpNetMessage netMsg = msg as UdpNetMessage;
            switch (netMsg.Cmd)
            {
                //ACK报文
                case KcpProtocol.ACK:
                    {
                        UdpNetMessage tmpMsg;
                        if (sndMsgDict.TryRemove(netMsg.SN, out tmpMsg))
                        {
#if DEBUG
                            Utility.Debug.LogInfo($" Conv :{Conv}，Receive KCP_ACK Message");
#endif
                            GameManager.ReferencePoolManager.Despawn(tmpMsg);
                        }
                        else
                        {
#if DEBUG

                            if (netMsg.Conv != 0)
                                Utility.Debug.LogError($"Receive KCP_ACK Message Exception；SN : {netMsg.SN} ");
#endif
                        }
                    }
                    break;
                case KcpProtocol.MSG:
                    {
#if DEBUG
                        Utility.Debug.LogInfo($"Conv : {Conv} ,Receive KCP_MSG ：{netMsg},消息体:{Utility.Converter.GetString(netMsg.ServiceMsg)}");
#endif
                        //生成一个ACK报文，并返回发送
                        var ack = UdpNetMessage.ConvertToACK(netMsg);
                        //这里需要发送ACK报文
                        sendMessageHandler?.Invoke(ack);
                        if (netMsg.OperationCode == InnerOpCode._Heartbeat)
                        {
                            Heartbeat.OnRenewal();
#if DEBUG
                            Utility.Debug.LogInfo($" Send KCP_ACK Message，conv :{Conv} ;  {PeerEndPoint.Address} ;{PeerEndPoint.Port}");
#endif
                        }
                        else
                        {
                            //发送后进行原始报文数据的处理
                            HandleMsgSN(netMsg);
                        }
                    }
                    break;
                case KcpProtocol.SYN:
                    {
                        //建立连接标志
                        Utility.Debug.LogInfo($"Conv : {Conv} ,Receive KCP_SYN Message");
                        //生成一个ACK报文，并返回发送
                        var ack = UdpNetMessage.ConvertToACK(netMsg);
                        //这里需要发送ACK报文
                        sendMessageHandler?.Invoke(ack);
                    }
                    break;
                case KcpProtocol.FIN:
                    {
                        //结束建立连接Cmd，这里需要谨慎考虑；
                        Utility.Debug.LogInfo($"Conv : {Conv} ,Receive KCP_FIN Message");
                        var ack = UdpNetMessage.ConvertToACK(netMsg);
                        //这里需要发送ACK报文
                        sendMessageHandler?.Invoke(ack);
                        AbortConnection();
                    }
                    break;
            }
        }
        /// <summary>
        /// 轮询更新，创建Peer对象时候将此方法加入监听；
        /// </summary>
        /// <param name="service">服务端的Peer</param>
        public void OnRefresh()
        {
            Heartbeat?.OnRefresh();
            long now = Utility.Time.MillisecondNow();
            if (now <= latestPollingTime)
                return;
            latestPollingTime = now + interval;
            if (!Available)
                return;
            foreach (var msg in sndMsgDict.Values)
            {
                if (msg.RecurCount >= 20)
                {
                    Available = false;
                    Utility.Debug.LogInfo($"Peer Conv:{Conv }  Unavailable");
                    AbortConnection();
                    return;
                }
                var time = Utility.Time.MillisecondTimeStamp() - msg.TS;
                if (time >= (msg.RecurCount + 1) * interval)
                {
                    //重发次数+1
                    msg.RecurCount += 1;
                    //超时重发
                    sendMessageHandler?.Invoke(msg);
                    //Utility.Debug.LogInfo($"Peer Conv:{Conv }  ; {msg.ToString()}");
                }
            }
        }
        /// <summary>
        /// 对网络消息进行编码
        /// </summary>
        /// <param name="netMsg">生成的消息</param>
        /// <returns>是否编码成功</returns>
        public bool EncodeMessage(ref UdpNetMessage netMsg)
        {
            netMsg.TS = Utility.Time.MillisecondTimeStamp();
            SendSN += 1;
            netMsg.SN = SendSN;
            netMsg.Snd_nxt = SendSN + 1;
            //netMsg.EncodeMessage();
            bool result = true;
            if (Conv != 0)
            {
                try
                {
                    if (netMsg.Cmd == KcpProtocol.MSG)
                        sndMsgDict.TryAdd(netMsg.SN, netMsg);
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
                //若会话ID不为0，则缓存入ACK容器中，等接收成功后进行移除
                ///*   result=*/ ackMsgDict.TryAdd(netMsg.SN, netMsg);
            }
            return result;
        }
        /// <summary>
        /// 终止连接
        /// </summary>
        public void AbortConnection()
        {
            Available = false;
            abortPeerHandler?.Invoke(Conv);
        }
        public void Clear()
        {
            Available = false;
            Conv = 0;
            PeerEndPoint = null;
            HandleSN = 0;
            SendSN = 0;
            latestPollingTime = 0;
            sendMessageHandler = null;
            sndMsgDict.Clear();
            Heartbeat.Clear();
            abortPeerHandler = null;
        }
        public override string ToString()
        {
            string str = $"Peer Conv  {Conv} ,PeerEndPoint :{PeerEndPoint.Address} , {PeerEndPoint.Port}";
            return str;
        }
        /// <summary>
        /// 处理报文序号
        /// </summary>
        /// <param name="netMsg">网络消息</param>
        protected void HandleMsgSN(UdpNetMessage netMsg)
        {
            //sn小于当前处理HandleSN则表示已经处理过的消息；
            if (netMsg.SN <= HandleSN)
            {
                return;
            }
            if (netMsg.SN - HandleSN > 1)
            {
                //对错序报文进行缓存
                rcvMsgDict.TryAdd(netMsg.SN, netMsg);
            }
            HandleSN = netMsg.SN;
            NetworkMsgEventCore.Instance.Dispatch(netMsg.OperationCode, netMsg);
#if DEBUG
            Utility.Debug.LogWarning($"Peer Conv:{Conv}， HandleMsgSN : {netMsg.ToString()}");
#endif
            UdpNetMessage nxtNetMsg;
            if (rcvMsgDict.TryRemove(HandleSN + 1, out nxtNetMsg))
            {
#if DEBUG
                Utility.Debug.LogInfo($"HandleMsgSN Next KCP_MSG : {netMsg.ToString()}");
#endif
                HandleMsgSN(nxtNetMsg);
            }
        }
    }
}
