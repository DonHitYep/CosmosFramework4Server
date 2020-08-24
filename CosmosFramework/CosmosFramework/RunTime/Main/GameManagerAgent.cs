using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public class GameManagerAgent : ConcurrentSingleton<GameManagerAgent>, IRefreshable
    {
        bool isPause = false;
        public bool Pause
        {
            get { return isPause; }
            set
            {
                if (isPause == value)
                    return;
                isPause = value;
                if (isPause)
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
                if (isPause)
                    return;
                foreach (KeyValuePair<ModuleEnum, IModule> module in moduleDict)
                {
                    module.Value?.OnRefresh();
                }
            }
        }
        void OnPause()
        {
            foreach (KeyValuePair<ModuleEnum, IModule> module in moduleDict)
            {
                module.Value.OnPause();
            }
        }
        void OnUnPause()
        {
            foreach (KeyValuePair<ModuleEnum, IModule> module in moduleDict)
            {
                module.Value.OnUnPause();
            }
        }
    }
}
