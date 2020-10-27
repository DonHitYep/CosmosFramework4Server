using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;
using Cosmos;
namespace CosmosServer
{
    public class ImplMessagePackHelper : IMessagePackHelper
    {
        public byte[] ToByteArray<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj);
        }
        public string ToJson<T>(T obj)
        {
            return MessagePackSerializer.SerializeToJson(obj);
        }
        public T ToObject<T>(byte[] buffer)
        {
            return MessagePackSerializer.Deserialize<T>(buffer);
        }
        public object ToObject(byte[] buffer, Type objectType)
        {
            return MessagePackSerializer.Deserialize(objectType, buffer);
        }
        public object ToObject(string json, Type objectType)
        {
            var bytes = MessagePackSerializer.ConvertFromJson(json);
            return MessagePackSerializer.Deserialize(objectType, bytes);
        }
        public T ToObject<T>(string json)
        {
            var bytes = MessagePackSerializer.ConvertFromJson(json);
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
    }
}
