using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Polling
{
    public class PollingManager : Module<PollingManager>, IRefreshable, IControllable
    {
        Action pollingHandler;
        public override void OnInitialization()
        {
            IsPause = false;
        }
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
        public override void OnRefresh()
        {
            if (IsPause)
                return;
            pollingHandler?.Invoke();
        }
    }
}
