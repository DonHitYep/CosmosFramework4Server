using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cosmos
{
    public class GameManagerAgent : ConcurrentSingleton<GameManagerAgent>, IRefreshable, IControllable
    {
        public bool IsPause { get; private set; }
        public bool Pause
        {
            get { return IsPause; }
            set
            {
                if (IsPause == value)
                    return;
                IsPause = value;
                if (IsPause)
                {
                    OnPause();
                }
                else
                {
                    OnUnPause();
                }
            }
        }
        public void Start()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    OnRefresh();
                }
                catch (System.Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
        }

        public void OnRefresh()
        {
            if (!IsPause)
                GameManager.Instance.OnRefresh();
        }
        public void OnPause()
        {
            GameManager.Instance.OnPause();
        }
        public void OnUnPause()
        {
            GameManager.Instance.OnUnPause();
        }
    }
}
