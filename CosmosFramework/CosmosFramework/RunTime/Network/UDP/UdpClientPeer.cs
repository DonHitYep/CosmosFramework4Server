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
    public class UdpClientPeer : IRemotePeer
    {
        public uint Conv { get;private set; }
        public IPEndPoint PeerEndPoint { get; private set; }
        public UdpClient ServerPeer { get; set; }
        /// <summary>
        /// 处理的message序号，按1累次递增。
        /// </summary>
        public uint HandleSN { get; set; }
        /// <summary>
        /// 已经发送的消息序号
        /// </summary>
        public uint SendSN { get; set; }
        /// <summary>
        /// 当前Peer是否失效
        /// </summary>
        public bool IsAbort { get; private set; } = false;
        /// <summary>
        /// 并发发送消息的字典；
        /// 整理错序报文；
        /// </summary>
        ConcurrentDictionary<uint, UdpNetworkMessage> msgDict = new ConcurrentDictionary<uint, UdpNetworkMessage>();
        /// <summary>
        /// ACK报文缓存，接收成功后从缓存中移除
        /// </summary>
        ConcurrentDictionary<uint, UdpNetworkMessage> ackMsgDict = new ConcurrentDictionary<uint, UdpNetworkMessage>();

        UdpClient udpClient;

        Action<UdpNetworkMessage> Handle;
        const int interval = 100;

        public UdpClientPeer(uint conv)
        {
            this.Conv = conv;
        }
        public void SetPeerEndPoint(IPEndPoint endPoint)
        {
            this.PeerEndPoint = endPoint;
        }
        public void MsgHandler(INetworkMessage msg)
        {
            UdpNetworkMessage netMsg =msg as UdpNetworkMessage;
            switch (netMsg.Cmd)
            {
                //ACK报文
                case KcpProtocol.ACK:
                    {
                        UdpNetworkMessage tmpMsg;
                        if (msgDict.TryRemove(netMsg.SN, out tmpMsg))
                        {

                        }
                        else
                        {

                        }
                    }
                    break;
                case KcpProtocol.MSG:
                    {
                        UdpNetworkMessage ackMsg = new UdpNetworkMessage(netMsg);

                        HandleNetMessage(netMsg);
                    }
                    break;
            }
        }
        void HandleNetMessage(UdpNetworkMessage netMsg)
        {
            if (netMsg.SN <= HandleSN)
            {
                return;
            }
            if (netMsg.SN - HandleSN > 1)
            {
                if (msgDict.TryAdd(netMsg.SN, netMsg))
                {
                    //收到错序报文
                }
                return;
            }
            HandleSN = netMsg.SN;
        }
      async void CheckOutTime()
        {
            await Task.Delay(interval);
            if (IsAbort)
                return;
            foreach (var msg in msgDict.Values)
            {
                if (msg.RecurCount >= 20)
                {
                    IsAbort = true;
                    return;
                }
                if (Utility.Time.MillisecondTimeStamp() - msg.TS>= (msg.RecurCount + 1) * interval)
                {
                    //重发次数+1
                    msg.RecurCount += 1;
                    //超时重发
                    NetworkEventCore.Instance.Dispatch(NetworkCode.SEND_MESSAGE, msg);
                }
            }
            CheckOutTime();
        }
        /// <summary>
        /// 发送网络消息包到远程
        /// </summary>
        /// <param name="netMsg">消息包</param>
      public void SendMsg(UdpNetworkMessage netMsg)
        {
            netMsg.TS = Utility.Time.MillisecondTimeStamp ();
            SendSN += 1;
            netMsg.SN = SendSN;
            netMsg.EncodeMessage();
            NetworkEventCore.Instance.Dispatch(NetworkCode.SEND_MESSAGE, netMsg);
            if (Conv != 0)
            {
                ackMsgDict.TryAdd(netMsg.SN, netMsg);
            }
        }
        /// <summary>
        /// 处理报文
        /// </summary>
        /// <param name="netMsg">网络消息</param>
      void HandleNetMsg(UdpNetworkMessage netMsg)
        {
            //sn小于当前处理HandleSN则表示已经处理过的消息；
            if (netMsg.SN <= HandleSN)
            {
                return;
            }
            if (netMsg.SN - HandleSN > 1)
            {
                //对错序报文进行缓存
                msgDict.TryAdd(netMsg.SN, netMsg);
                NetworkEventCore.Instance.Dispatch(netMsg.MsgID, netMsg);
            }
            HandleSN = netMsg.SN;
            UdpNetworkMessage nxtNetMsg;
            if(msgDict.TryRemove(HandleSN+1,out nxtNetMsg))
            {
                HandleNetMsg(nxtNetMsg);
            }
        }
    }
}
