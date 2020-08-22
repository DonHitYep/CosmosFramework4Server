using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    public class PollingManager:Module<PollingManager>,IRefreshable,IControllable
    {
        Action pollingHandler;

        public bool IsPause { get; private set; }
        public void AddPolling(Action handler)
        {
            try
            {
                pollingHandler += handler;
            }
            catch 
            {
                Utility.Debug.LogError("无法添加监听到轮询池中");
            }
        }
        public void RemovePolling(Action handler)
        {
            try
            {
                pollingHandler -= handler;
            }
            catch
            {
                Utility.Debug.LogError("无法从轮询池中移除监听");
            }
        }
        public  void OnRefresh()
        {
            while (true)
            {
                if (IsPause)
                    continue;
                pollingHandler?.Invoke();
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
