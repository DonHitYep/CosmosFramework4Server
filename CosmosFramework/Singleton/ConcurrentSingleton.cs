using System;
namespace Cosmos
{
    /// <summary>
    /// 多线程单例基类，内部包含线程锁
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ConcurrentSingleton<T> : IDisposable
              where T : ConcurrentSingleton<T>, new()
    {
        protected static T instance;
        static readonly object locker = new object();
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                            if (instance is IBehaviour)
                                (instance as IBehaviour).OnInitialization();
                        }
                    }
                }
                return instance;
            }
        }
        /// <summary>
        ///非空虚方法，IDispose接口
        /// </summary>
        public virtual void Dispose()
        {
            if (instance is IBehaviour)
                (instance as IBehaviour).OnTermination();
            instance = default(T);
        }
    }
}
