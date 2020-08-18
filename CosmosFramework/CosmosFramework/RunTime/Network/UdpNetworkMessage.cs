using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
namespace Cosmos
{
    public class UdpNetworkMessage : INetworkMessage
    {
        /// <summary>
        /// 消息大小
        /// </summary>
        public int MessageSize { get; private set; }
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
            MessageSize = message.Length;
            SessionID = sessionID;
            SessionNum = sessionNum;
            ModuleID = moduleID;
            MessageType = messageType;
            MessageID = messageID;
            Message = message;
        }
        public UdpNetworkMessage(byte[] buffer)
        {
            Buffer = buffer;
            DecodeMessage(Buffer);
        }
        public void DecodeMessage(byte[] buffer)
        {
            if (buffer.Length >= 4)
            {
                MessageSize = BitConverter.ToInt32(buffer, 0);
                if (buffer.Length == MessageSize + 32)
                {
                    IsFull = true;
                }
            }
            else
            {
                IsFull = false;
            }
            SessionID = BitConverter.ToInt32(buffer, 4);
            SessionNum = BitConverter.ToInt32(buffer, 8);
            ModuleID = BitConverter.ToInt32(buffer, 12);
            TimeStamp = BitConverter.ToInt64(buffer, 16);
            MessageType = BitConverter.ToInt32(buffer, 32);
            MessageID = BitConverter.ToInt32(buffer, 28);
            //MessageType = 0 表示为ACK报文
            if (MessageType != 0)
            {
                Message = new byte[MessageSize];
                Array.Copy(buffer, 32, Message, 0, MessageSize);
            }
        }
        /// <summary>
        /// 编码消息
        /// </summary>
        /// <returns>编码后的消息字节流</returns>
        public byte[] EncodeMessage()
        {
            byte[] data = new byte[32 + MessageSize];
            if (MessageType==0)
                MessageSize = 0;
            byte[] msgSize = BitConverter.GetBytes(MessageSize);
            byte[] sessionID = BitConverter.GetBytes(SessionID);
            byte[] sessionNum = BitConverter.GetBytes(SessionNum);
            byte[] moduleID = BitConverter.GetBytes(ModuleID);
            byte[] timeStamp = BitConverter.GetBytes(TimeStamp);
            byte[] msgType = BitConverter.GetBytes(MessageType);
            byte[] msgID = BitConverter.GetBytes(MessageID);
            Array.Copy(msgSize, 0, data, 0, 4);
            Array.Copy(sessionID, 0, data, 4, 4);
            Array.Copy(sessionNum, 0, data, 8, 4);
            Array.Copy(moduleID, 0, data, 12, 4);
            Array.Copy(timeStamp, 0, data, 16, 8);
            Array.Copy(msgType, 0, data, 24, 4);
            Array.Copy(msgID, 0, data, 28, 4);
            //如果不是ACK报文，则追加数据
            if (MessageType!=0)
                Array.Copy(Message, 0, data, 32, Message.Length);
            Buffer = data;
            return data;
        }
    }
}
