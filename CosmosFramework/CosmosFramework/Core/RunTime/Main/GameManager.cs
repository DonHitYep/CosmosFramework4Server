using Cosmos.Network;
using Cosmos.Polling;
using Cosmos.Reference;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public sealed partial class GameManager : ConcurrentSingleton<GameManager>, IControllable, IRefreshable
    {
        #region Properties
        public bool IsPause { get; private set; }
        /// <summary>
        /// 轮询更新委托
        /// </summary>
        internal Action refreshHandler;
        internal event Action RefreshHandler
        {
            add { refreshHandler += value; }
            remove { refreshHandler -= value; }
        }
        int moduleCount = 0;
        static ConcurrentDictionary<Type, IModule> moduleDict;
        internal static ConcurrentDictionary<Type, IModule> ModuleDict { get { return moduleDict; } }
        static ReferencePoolManager referencePoolManager;
        public static ReferencePoolManager ReferencePoolManager
        {
            get
            {
                if (referencePoolManager == null)
                {
                    referencePoolManager = new ReferencePoolManager();
                    Instance.ModuleInitialization(referencePoolManager);
                }
                return referencePoolManager;
            }
        }
        static NetworkManager networkManager;
        public static NetworkManager NetworkManager
        {
            get
            {
                if (networkManager == null)
                {
                    networkManager = new NetworkManager();
                    Instance.ModuleInitialization(networkManager);
                }
                return networkManager;
            }
        }
        static PollingManager pollingManager;
        public static PollingManager PollingManager
        {
            get
            {
                if (pollingManager == null)
                {
                    pollingManager = new PollingManager();
                    Instance.ModuleInitialization(pollingManager);
                }
                return pollingManager;
            }
        }
        #endregion
        #region Methods
        /// <summary>
        /// 构造函数，只有使用到时候才产生
        /// </summary>
        public GameManager()
        {
            if (moduleDict == null)
            {
                moduleDict = new ConcurrentDictionary<Type, IModule>();
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
        public void OnRefresh()
        {
            if (IsPause)
                return;
            refreshHandler?.Invoke();
        }
        /// <summary>
        /// 终结并释放GameManager
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            ModuleDict.Clear();
        }
        internal void ModuleInitialization(IModule module)
        {
            module.OnInitialization();
            var type = module.ModuleType;
            if (!HasModule(type))
            {
                moduleDict.TryAdd(type, module);
                moduleCount++;
                RefreshHandler += module.OnRefresh;
                Utility.Debug.LogInfo($"Module :{module} is OnInitialization");
            }
            else
            {
                Utility.Debug.LogError(new ArgumentException($"Module : {module} is already exist!"));
            }
        }
        internal void ModuleTermination(IModule module)
        {
            var type = module.ModuleType;
            if (HasModule(type))
            {
                module.OnTermination();
                RefreshHandler -= module.OnRefresh;
                moduleDict.TryRemove(type, out _);
                moduleCount--;
            }
            else
                Utility.Debug.LogError(new ArgumentException($"Module : {module} is not exist!"));
        }
        internal bool HasModule(Type type)
        {
            return moduleDict.ContainsKey(type);
        }
        #endregion
    }
}
