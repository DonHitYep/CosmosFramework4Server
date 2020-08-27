using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Network
{
    public class UdpClientPeer : IRemotePeer, IRefreshable
    {
        /// <summary>
        /// 会话ID
        /// </summary>
        public uint Conv { get; private set; }
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
        protected ConcurrentDictionary<uint, UdpNetworkMessage> ackMsgDict;
        /// <summary>
        /// 解析间隔
        /// </summary>
        protected const int interval = 2000;
        /// <summary>
        /// 发送网络消息委托；
        /// 这里函数指针指向service的sendMessage
        /// </summary>
        Action<INetworkMessage> sendMessageHandler;
        public UdpClientPeer()
        {
            ackMsgDict = new ConcurrentDictionary<uint, UdpNetworkMessage>();
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
        public void SetValue(Action<INetworkMessage> sendMsgCallback, uint conv, IPEndPoint endPoint)
        {
            this.Conv = conv;
            this.PeerEndPoint = endPoint;
            latestPollingTime = Utility.Time.MillisecondNow() + interval;
            this.sendMessageHandler = sendMsgCallback;
            Available = true;
        }
        /// <summary>
        /// 处理进入这个peer的消息
        /// </summary>
        /// <param name="service">udp服务</param>
        /// <param name="msg">消息体</param>
        public virtual void MessageHandler(INetworkMessage msg)
        {
            UdpNetworkMessage netMsg = msg as UdpNetworkMessage;
            switch (netMsg.Cmd)
            {
                //ACK报文
                case KcpProtocol.ACK:
                    {
                        UdpNetworkMessage tmpMsg;
                        if (ackMsgDict.TryRemove(netMsg.SN, out tmpMsg))
                        {
                            Utility.Debug.LogInfo($" Conv :{Conv}，接收到ACK消息 ");
                        }
                        else
                        {
                            if (netMsg.Conv != 0)
                                Utility.Debug.LogError($"接收网络ACK消息异常；SN : {netMsg.SN} ");
                        }
                    }
                    break;
                case KcpProtocol.MSG:
                    {
                        Utility.Debug.LogInfo($"Conv : {Conv} ,接收到MSG消息");
                        //生成一个ACK报文，并返回发送
                        var ack = UdpNetworkMessage.ConvertToACK(netMsg);
                        //这里需要发送ACK报文
                        sendMessageHandler?.Invoke(netMsg);
                        //发送后进行原始报文数据的处理
                        HandleMsgSN(netMsg);
                        Utility.Debug.LogInfo($"发送ACK报文，conv :{Conv} ;  {PeerEndPoint.Address} ;{PeerEndPoint.Port}");
                        if (netMsg.OperationCode == OperationCode._Heartbeat)
                            Heartbeat.OnRenewal();
                        else
                            NetworkEventCore.Instance.Dispatch(netMsg.OperationCode, netMsg);
                    }
                    Utility.Debug.LogInfo($"当前消息缓存数量为:{ackMsgDict.Count} ; Peer conv : {Conv}");
                    break;
                case KcpProtocol.FIN:
                    {
                        //结束建立连接Cmd，这里需要谨慎考虑；
                        Utility.Debug.LogWarning($"Conv : {Conv} ,接收到FIN消息");
                        AbortConnection();
                    }
                    break;
                case KcpProtocol.SYN:
                    {
                        //建立连接标志
                        Utility.Debug.LogInfo($"Conv : {Conv} ,接收到SYN消息");
                        //生成一个ACK报文，并返回发送
                        var ack = UdpNetworkMessage.ConvertToACK(netMsg);
                        //这里需要发送ACK报文
                        sendMessageHandler?.Invoke(netMsg);
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
            long now = Utility.Time.MillisecondNow();
            if (now <= latestPollingTime)
                return;
            latestPollingTime = now + interval;
            if (!Available)
                return;
            Heartbeat?.OnRefresh();
            foreach (var msg in ackMsgDict.Values)
            {
                if (msg.RecurCount >= 30)
                {
                    Available = false;
                    Utility.Debug.LogInfo($"Peer Conv:{Conv } 失去连接");
                    return;
                }
                var time = Utility.Time.MillisecondTimeStamp() - msg.TS;
                if (time >= (msg.RecurCount + 1) * interval)
                {
                    //重发次数+1
                    msg.RecurCount += 1;
                    //超时重发
                    sendMessageHandler?.Invoke(msg);
                }
            }
        }
        /// <summary>
        /// 对网络消息进行编码
        /// </summary>
        /// <param name="netMsg">生成的消息</param>
        /// <returns>是否编码成功</returns>
        public bool EncodeMessage(ref UdpNetworkMessage netMsg)
        {
            netMsg.TS = Utility.Time.MillisecondTimeStamp();
            SendSN += 1;
            netMsg.SN = SendSN;
            netMsg.Snd_nxt = SendSN + 1;
            netMsg.EncodeMessage();
            bool result = true;
            if (Conv != 0)
            {
                try
                {
                    if (netMsg.Cmd == KcpProtocol.MSG)
                        ackMsgDict.TryAdd(netMsg.SN, netMsg);
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
                //若会话ID不为0，则缓存入ACK容器中，等接收成功后进行移除
                ///*   result=*/ ackMsgDict.TryAdd(netMsg.SN, netMsg);
            }
            return result; ;
        }
        /// <summary>
        /// 终止连接
        /// </summary>
        public void AbortConnection()
        {
            Available = false;
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
            ackMsgDict.Clear();
        }
        /// <summary>
        /// 处理报文序号
        /// </summary>
        /// <param name="netMsg">网络消息</param>
        protected void HandleMsgSN(UdpNetworkMessage netMsg)
        {
            //sn小于当前处理HandleSN则表示已经处理过的消息；
            if (netMsg.SN <= HandleSN)
            {
                return;
            }
            if (netMsg.SN - HandleSN > 1)
            {
                //对错序报文进行缓存
                ackMsgDict.TryAdd(netMsg.SN, netMsg);
                Utility.Debug.LogWarning($"缓存报文 HandleSN : {HandleSN}");
            }
            HandleSN = netMsg.SN;
            Utility.Debug.LogWarning($"Peer Conv:{Conv}，处理的消息 HandleSN : {HandleSN}");
            UdpNetworkMessage nxtNetMsg;
            if (ackMsgDict.TryRemove(HandleSN + 1, out nxtNetMsg))
            {
                HandleMsgSN(nxtNetMsg);
            }
        }
 
    }
}
