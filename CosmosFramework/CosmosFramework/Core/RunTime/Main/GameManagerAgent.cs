using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public class GameManagerAgent : ConcurrentSingleton<GameManagerAgent>, IRefreshable,IControllable
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
        Dictionary<ModuleEnum, IModule> moduleDict;
        public GameManagerAgent()
        {
            moduleDict = GameManager.ModuleDict;
        }
        public void OnRefresh()
        {
            while (true)
            {
                if (!IsPause)
                    GameManager.Instance.OnRefresh();
            }
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
