using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Polling
{
    public class PollingManager : Module<PollingManager>, IRefreshable, IControllable
    {
        Action pollingHandler;
        public event Action PollingHandler
        {
            add { pollingHandler +=value; }
            remove{pollingHandler -= value;}
        }
        public override void OnInitialization()
        {
            IsPause = false;
        }
        public override void OnRefresh()
        {
            if (IsPause)
                return;
            pollingHandler?.Invoke();
        }
    }
}
