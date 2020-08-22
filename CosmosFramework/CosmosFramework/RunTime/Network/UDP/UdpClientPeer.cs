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
        public bool IsConnect { get; private set; } = false;
        /// <summary>
        /// 并发发送消息的字典；
        /// 整理错序报文；
        /// 临时起到ACK缓存的作用
        /// </summary>
        ConcurrentDictionary<uint, UdpNetworkMessage> msgDict = new ConcurrentDictionary<uint, UdpNetworkMessage>();
        const int interval = 100;
        public UdpClientPeer(uint conv)
        {
            this.Conv = conv;
        }
        public void SetPeerEndPoint(IPEndPoint endPoint)
        {
            this.PeerEndPoint = endPoint;
        }
        public void MsgHandler(UdpServerService service,INetworkMessage msg)
        {
            UdpNetworkMessage netMsg =msg as UdpNetworkMessage;
            switch (netMsg.Cmd)
            {
                //ACK报文
                case KcpProtocol.ACK:
                    {
                        UdpNetworkMessage tmpMsg;
                        if (!msgDict.TryRemove(netMsg.SN, out tmpMsg))
                            Utility.Debug.LogError($"网络消息ACK接收异常 :{tmpMsg.SN} ");
                        else
                        {
                            Utility.Debug.LogInfo($"网络消息 : ID :{Conv},内容：{Utility.Encode.ConvertToString(tmpMsg.ServiceMsg)} ");
                        }
                    }
                    break;
                case KcpProtocol.MSG:
                    {
                        //生成一个ACK报文，并返回发送
                        netMsg.ConvertToACK();
                        //这里需要发送ACK报文
                        service.SendMessage(netMsg, PeerEndPoint);
                        //发送后进行原始报文数据的处理
                        HandleMsgSN(netMsg);
                    }
                    break;
            }
        }
        /// <summary>
        /// 轮询更新，创建Peer对象时候将此方法加入监听；
        /// </summary>
        /// <param name="service">服务端的Peer</param>
        public async void OnPolling(UdpServerService service)
        {
            await Task.Delay(interval);
            if (!IsConnect)
                return;
            foreach (var msg in msgDict.Values)
            {
                if (msg.RecurCount >= 20)
                {
                    IsConnect = false;
                    return;
                }
                if (Utility.Time.MillisecondTimeStamp() - msg.TS >= (msg.RecurCount + 1) * interval)
                {
                    //重发次数+1
                    msg.RecurCount += 1;
                    //超时重发
                    service.SendMessage(msg,PeerEndPoint);
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
            netMsg.EncodeMessage();
            bool result=false;
            if (Conv != 0)
            {
                //若会话ID不为0，则缓存入ACK容器中，等接收成功后进行移除
                result= msgDict.TryAdd(netMsg.SN, netMsg);
            }
            return  result; ;
        }
        public void Close()
        {
            IsConnect = false;   
        }
        /// <summary>
        /// 处理报文序号
        /// </summary>
        /// <param name="netMsg">网络消息</param>
      void HandleMsgSN(UdpNetworkMessage netMsg)
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
            }
            HandleSN = netMsg.SN;
            UdpNetworkMessage nxtNetMsg;
            if(msgDict.TryRemove(HandleSN+1,out nxtNetMsg))
            {
                HandleMsgSN(nxtNetMsg);
            }
        }
    }
}
