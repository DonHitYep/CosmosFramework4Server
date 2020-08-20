using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cosmos
{
    public class WaitTimeCoroutine
    {
        public WaitTimeCoroutine(int time, Action handler)
        {
            Time = time;
            Handler = handler;
        }
        public int Time { get; private set; }
        public Action Handler { get; private set; }
    }
    /// <summary>
    /// 单线程异步携程
    /// 由外部开启OnRefresh；
    /// </summary>
    public class SingleThreadAsyncCoroutine : Singleton<SingleThreadAsyncCoroutine>, IRefreshable, IControllable
    {
        public bool IsPause { get; set; } = false;
        public int CorouCount { get { return corouDict.Count; } }
       //ConcurrentDictionary <long, Action> corouDict = new ConcurrentDictionary<long, Action>();
       Dictionary <long, Action> corouDict = new Dictionary<long, Action>();
        HashSet<long> removeSet = new HashSet<long>();
        public void Run()
        {
            while (true)
            {
                if (!IsPause)
                {
                    OnRefresh();
                }
            }
        }
        public void OnRefresh()
        {
            long nowTicks = Utility.Time.MillisecondNow();

            if (corouDict.Count <= 0)
                return;
            removeSet.Clear();
            foreach (var corou in corouDict)
            {
                if (corou.Key >= nowTicks)
                {
                    corou.Value.Invoke();
                    removeSet.Add(corou.Key);
                }
            }
            foreach (var key in removeSet)
            {
                if (corouDict.ContainsKey(key))
                {
                    //Action act;
                    //corouDict.TryRemove(key,out act);

                    corouDict.Remove(key);
                }
            }
        }
        public void WaitTimeAsyncCallback(WaitTimeCoroutine coroutine)
        {
            WaitTimeAsyncCallback(coroutine.Time, coroutine.Handler);
        }
        /// <summary>
        /// 单线程异步委托
        /// </summary>
        /// <param name="waitTime">等待时间，这里是ms</param>
        /// <param name="callback">回调</param>
        public void WaitTimeAsyncCallback(int waitTime, Action callback)
        {
            var dispatchTime = Utility.Time.MillisecondNow()+ waitTime;
            if (corouDict.ContainsKey(dispatchTime))
            {
                corouDict[dispatchTime] += callback;
            }
            else
            {
                corouDict.TryAdd(dispatchTime, callback);
            }
        }

        public void OnPause()
        {
            IsPause = true;
        }
        public void OnUnPause()
        {
            IsPause = false;
        }
    }
}
