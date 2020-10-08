using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public interface IMessagePackHelper
    {
        string ToJson<T>(T obj);
        byte[] ToByteArray(object obj);
        T ToObject<T>(byte[] buffer);
        object ToObject(byte[] buffer , Type objectType);
    }
}
