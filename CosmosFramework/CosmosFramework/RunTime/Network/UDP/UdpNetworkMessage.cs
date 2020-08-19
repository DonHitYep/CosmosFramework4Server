using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace Cosmos
{
    public class UdpNetworkMessage : INetworkMessage,IReference
    {
        /// <summary>
        /// 消息包体大小；
        /// 取值范围0~65535；
        /// 约64K一个包
        /// </summary>
        public ushort MessageSize { get; private set; }
        /// <summary>
        /// 会话ID
        /// </summary>
        public int SessionID { get; set; }
        /// <summary>
        /// 会话序号
        /// </summary>
        public int SessionNum { get; set; }
        /// <summary>
        /// 模块ID
        /// </summary>
        public int ModuleID { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public long TimeStamp { get; set; }
        /// <summary>
        /// 协议类型
        /// </summary>
        public int MessageType { get; set; }
        /// <summary>
        /// 协议ID
        /// </summary>
        public int MessageID { get; set; }
        /// <summary>
        /// 业务报文
        /// </summary>
        public byte[] Message { get; set; }
        /// <summary>
        /// 收发的数组
        /// </summary>
        public byte[] Buffer { get; set; }
        /// <summary>
        /// 是否是完整报文
        /// </summary>
        public bool IsFull { get; private set; }
        public UdpNetworkMessage(int sessionID, int sessionNum, int moduleID, int messageType, int messageID, byte[] message)
        {
            MessageSize = (ushort)message.Length;
            SessionID = sessionID;
            SessionNum = sessionNum;
            ModuleID = moduleID;
            MessageType = messageType;
            MessageID = messageID;
            Message = message;
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
                MessageSize = BitConverter.ToUInt16(buffer, 0);
                if (buffer.Length == MessageSize + 30)
                {
                    IsFull = true;
                }
            }
            else
            {
                IsFull = false;
            }
            SessionID = BitConverter.ToInt32(buffer, 2);
            SessionNum = BitConverter.ToInt32(buffer, 6);
            ModuleID = BitConverter.ToInt32(buffer, 10);
            TimeStamp = BitConverter.ToInt64(buffer, 14);
            MessageType = BitConverter.ToInt32(buffer, 22);
            MessageID = BitConverter.ToInt32(buffer, 26);
            //MessageType = 0 表示为ACK报文
            if (MessageType != 0)
            {
                Message = new byte[MessageSize];
                Array.Copy(buffer, 30, Message, 0, MessageSize);
            }
        }
        /// <summary>
        /// 编码UDP报文消息
        /// </summary>
        /// <returns>编码后的消息字节流</returns>
        public byte[] EncodeMessage()
        {
            byte[] data = new byte[30 + MessageSize];
            if (MessageType==0)
                MessageSize = 0;
            byte[] msgSize = BitConverter.GetBytes(MessageSize);
            byte[] sessionID = BitConverter.GetBytes(SessionID);
            byte[] sessionNum = BitConverter.GetBytes(SessionNum);
            byte[] moduleID = BitConverter.GetBytes(ModuleID);
            byte[] timeStamp = BitConverter.GetBytes(TimeStamp);
            byte[] msgType = BitConverter.GetBytes(MessageType);
            byte[] msgID = BitConverter.GetBytes(MessageID);
            Array.Copy(msgSize, 0, data, 0, 2);
            Array.Copy(sessionID, 0, data, 2, 4);
            Array.Copy(sessionNum, 0, data, 6, 4);
            Array.Copy(moduleID, 0, data, 10, 4);
            Array.Copy(timeStamp, 0, data, 14, 8);
            Array.Copy(msgType, 0, data, 22, 4);
            Array.Copy(msgID, 0, data, 26, 4);
            //如果不是ACK报文，则追加数据
            if (MessageType!=0)
                Array.Copy(Message, 0, data, 30, Message.Length);
            Buffer = data;
            return data;
        }
        public void Clear()
        {
            MessageSize = 0;
            SessionID = 0;
            SessionNum = 0;
            ModuleID = 0;
            MessageType = 0;
            MessageID = 0;
            Message = null;
            IsFull = false;
            TimeStamp = 0;
        }
    }
}
