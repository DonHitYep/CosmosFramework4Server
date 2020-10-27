using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;

namespace Cosmos.Reference
{
    internal sealed class ReferenceSpawnPool
    {
        ConcurrentQueue<IReference> referenceQueue = new ConcurrentQueue<IReference>();
        internal int ReferenceCount { get { return referenceQueue.Count; } }
        internal IReference Spawn(Type type)
        {
            IReference refer;
            if (referenceQueue.Count > 0)
            {
                referenceQueue.TryDequeue(out var re);
                refer = re;
            }
            else
                refer = Utility.Assembly.GetTypeInstance(type) as IReference;
            return refer;
        }
        internal IReference Spawn<T>()
            where T : class, IReference, new()
        {
            IReference refer=null;
            if (referenceQueue.Count > 0)
                if (referenceQueue.Count > 0)
                {
                    referenceQueue.TryDequeue(out var re);
                    refer = re;
                }
                else
                    refer = new T() as IReference;
            return refer;
        }
        internal void Despawn(IReference refer)
        {
            if (referenceQueue.Count >= ReferencePoolManager._ReferencePoolCapcity)
                refer = null;
            else
            {
                refer.Clear();
                referenceQueue.Enqueue(refer);
            }
        }
        internal void Clear()
        {
            referenceQueue.Clear();
        }
    }
}