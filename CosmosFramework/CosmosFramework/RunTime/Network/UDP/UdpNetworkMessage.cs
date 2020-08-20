using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace Cosmos
{
    public class UdpNetworkMessage : INetworkMessage, IReference
    {
        /// <summary>
        /// 消息包体大小；
        /// 取值范围0~65535；
        /// 约64K一个包
        /// </summary>
        public ushort Length { get; private set; }
        /// <summary>
        /// 会话ID;
        /// 为一个表示会话编号的整数，
        /// 和TCP的 conv一样，通信双方需保证 conv相同，
        /// 相互的数据包才能够被接受。conv唯一标识一个会话，但通信双方可以同时存在多个会话。
        /// </summary>
        public uint Conv { get; set; }
        /// <summary>
        /// 第一个未确认的包
        /// </summary>
        public uint Snd_una { get; set; }
        ///// <summary>
        /////下一个需要发送的msg序号
        ///// </summary>
        //public uint Snd_nxt { get; set; }
        /// <summary>
        /// 待接收消息序号；
        /// 这里填充ACK序号；
        /// </summary>
        public uint Rcv_nxt { get; set; }
        /// <summary>
        ///当前 message序号，按1累次递增。
        /// </summary>
        public uint SN { get; set; }
        /// <summary>
        /// 时间戳TimeStamp
        /// </summary>
        public long TS { get; set; }
        /// <summary>
        /// Kcp协议类型:
        /// 用来区分分片的作用。
        /// IKCP_CMD_PUSH：数据分片；
        /// IKCP_CMD_ACK：ack分片； 
        /// IKCP_CMD_WASK：请求告知窗口大小；
        /// IKCP_CMD_WINS：告知窗口大小。
        /// </summary>
        public ushort Cmd { get; set; }
        /// <summary>
        /// 业务报文
        /// </summary>
        public byte[] ServiceMsg { get; set; }

        /// <summary>
        /// 消息重传次数；
        /// 标准KCP库中（C版）重传上线是20；
        /// </summary>
        public ushort RecurCount { get; set; }
        /// <summary>
        /// 存储消息字节流的内存
        /// </summary>
        public byte[] Buffer { get; set; }
        /// <summary>
        /// 是否是完整报文
        /// </summary>
        public bool IsFull { get; private set; }
        /// <summary>
        /// 消息报文构造
        /// </summary>
        /// <param name="conv">会话ID</param>
        /// <param name="sn">msg序号</param>
        /// <param name="cmd">协议ID</param>
        /// <param name="message">消息内容</param>
        public UdpNetworkMessage(uint conv, uint sn, ushort cmd, byte[] message)
        {
            Length = (ushort)message.Length;
            Conv = conv;
            SN = sn;
            Cmd = cmd;
            ServiceMsg = message;
            Rcv_nxt = sn;
        }
        public UdpNetworkMessage(UdpNetworkMessage udpNetMsg)
        {
            Length = 0;
            Conv = udpNetMsg.Conv;
            SN = udpNetMsg.SN;
            Rcv_nxt = SN;
            Cmd = KcpProtocol.ACK; 
        }
        /// <summary>
        /// ACK报文构造
        /// </summary>
        /// <param name="conv">会话ID</param>
        /// <param name="snd_una">未确认的报文</param>
        /// <param name="sn">msgID</param>
        /// <param name="cmd">协议</param>
        public UdpNetworkMessage(uint conv, uint snd_una,uint sn, ushort cmd)
        {
            Conv = conv;
            Snd_una = snd_una;
            SN = sn;
            Rcv_nxt = SN;
            Cmd = cmd;
        }
        public UdpNetworkMessage() { }
        public UdpNetworkMessage(byte[] buffer)
        {
            Buffer = buffer;
            DecodeMessage(Buffer);
        }
        /// <summary>
        /// 解析UDP数据报文
        /// </summary>
        /// <param name="buffer"></param>
        public void DecodeMessage(byte[] buffer)
        {
            if (buffer.Length >= 2)
            {
                Length = BitConverter.ToUInt16(buffer, 0);
                if (buffer.Length == Length + 26)
                {
                    IsFull = true;
                }
            }
            else
            {
                IsFull = false;
            }
            Conv = BitConverter.ToUInt32(buffer, 2);
            Snd_una = BitConverter.ToUInt32(buffer, 6);
            Rcv_nxt= BitConverter.ToUInt32(buffer, 10);
            SN= BitConverter.ToUInt32(buffer, 14);
            TS= BitConverter.ToInt64(buffer, 18);
            Cmd = BitConverter.ToUInt16(buffer, 26);
            if (Cmd == KcpProtocol.MSG)
            {
                ServiceMsg = new byte[Length];
                Array.Copy(buffer, 26, ServiceMsg, 0, Length);
            }
        }
        /// <summary>
        /// 编码UDP报文消息
        /// </summary>
        /// <returns>编码后的消息字节流</returns>
        public byte[] EncodeMessage()
        {
            byte[] data = new byte[26 + Length];
            if (Cmd == KcpProtocol.ACK)
                Length = 0;
            byte[] len = BitConverter.GetBytes(Length);
            byte[] conv = BitConverter.GetBytes(Conv);
            byte[] snd_una= BitConverter.GetBytes(Snd_una);
            byte[] rcv_nxt= BitConverter.GetBytes(Rcv_nxt);
            byte[] sn = BitConverter.GetBytes(SN);
            byte[] ts = BitConverter.GetBytes(TS);
            byte[] cmd = BitConverter.GetBytes(Cmd);
            Array.Copy(len, 0, data, 0, 2);
            Array.Copy(conv, 0, data, 2, 4);
            Array.Copy(snd_una, 0, data, 6, 4);
            Array.Copy(rcv_nxt, 0, data, 10, 4);
            Array.Copy(sn, 0, data, 14, 4);
            Array.Copy(ts, 0, data, 18, 8);
            Array.Copy(cmd, 0, data, 26, 2);
            //如果不是ACK报文，则追加数据
            if (Cmd == KcpProtocol.MSG)
                Array.Copy(ServiceMsg, 0, data, 28, ServiceMsg.Length);
            Buffer = data;
            return data;
        }
        public void Clear()
        {
            Length = 0;
            Conv = 0;
            Snd_una = 0;
            Rcv_nxt = 0;
            SN = 0;
            TS = 0;
            Cmd = KcpProtocol.NIL;
            ServiceMsg = null;
            IsFull = false;
        }
    }
}
