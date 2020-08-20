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
        public int Conv { get; set; }
        public IPEndPoint EndPoint { get; private set; }
        /// <summary>
        /// 处理的message序号，按1累次递增。
        /// </summary>
        public uint HandleSN { get; set; }
        /// <summary>
        /// 已经发送的消息序号
        /// </summary>
        public uint SendSN { get; set; }
        ConcurrentDictionary<uint, UdpNetworkMessage> msgDict = new ConcurrentDictionary<uint, UdpNetworkMessage>();
        /// <summary>
        /// ACK报文缓存，接收成功后从缓存中移除
        /// </summary>
        ConcurrentDictionary<uint, UdpNetworkMessage> ackDict = new ConcurrentDictionary<uint, UdpNetworkMessage>();


        UdpClient udpClient;

        Action<UdpNetworkMessage> Handle;
        const int interval = 100;
        public void Handler(INetworkMessage netMsg)
        {
            var udpNetMsg = netMsg as UdpNetworkMessage;
            switch (udpNetMsg.Cmd)
            {
                //ACK报文
                case KcpProtocol.ACK:
                    {
                        UdpNetworkMessage msg;
                        if (msgDict.TryRemove(udpNetMsg.SN, out msg))
                        {

                        }
                        else
                        {
                        }
                    }
                    break;
                case KcpProtocol.MSG:
                    {
                        UdpNetworkMessage ackMsg = new UdpNetworkMessage(udpNetMsg);
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
            foreach (var msg in msgDict.Values)
            {
                if (msg.RecurCount >= 20)
                {
                    //uSocket.RemoveClient(session);
                    return;
                }
                if (Utility.Time.MillisecondTimeStamp() - msg.TS>= (msg.RecurCount + 1) * interval)
                {
                    //重发次数+1
                    msg.RecurCount += 1;
                    //Debug.Log($"超时重发,序号是:{package.sn}");
                    //uSocket.Send(msg.Buffer, endPoint);
                }
            }
        }
        void SendMsg(UdpNetworkMessage netMsg)
        {
            netMsg.TS = Utility.Time.MillisecondTimeStamp ();
            SendSN += 1;
            netMsg.SN = SendSN;
            netMsg.EncodeMessage();
            if (netMsg.Conv != 0)
            {
            }
        }
    }
}
