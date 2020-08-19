using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// 并发变量容器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentVariable<T> :Variable
        where T : new()
    {
        T data = new T();
        public T Data
        {
            get
            {
                lock (this)
                {
                    return data;
                }
            }
            set
            {
                lock (this)
                {
                    data = value;
                }
            }
        }
        Type dataType = typeof(T);
        public override Type Type { get { return dataType; } }
    }
}
