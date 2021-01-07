using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// Cosmos服务端入口；
    /// </summary>
    public class CosmosEntry : ConcurrentSingleton<CosmosEntry>
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
        public static IFSMManager FSMManager { get; private set; }
        public static IConfigManager ConfigManager { get; private set; }
        public static INetworkManager NetworkManager { get; private set; }
        public static IReferencePoolManager ReferencePoolManager { get; private set; }
        public static IEventManager EventManager { get; private set; }
        public void Start()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var length = assemblies.Length;
            for (int i = 0; i < length; i++)
            {
                var helper = Utility.Assembly.GetInstanceByAttribute<ImplementProviderAttribute, IDebugHelper>(assemblies[i]);
                if (helper != null)
                {
                    Utility.Debug.SetHelper(helper);
                    break;
                }
            }
            for (int i = 0; i < length; i++)
            {
                var helper = Utility.Assembly.GetInstanceByAttribute<ImplementProviderAttribute, IJsonHelper>(assemblies[i]);
                if (helper != null)
                {
                    Utility.Json.SetHelper(helper);
                    break;
                }
            }
            for (int i = 0; i < length; i++)
            {
                var helper = Utility.Assembly.GetInstanceByAttribute<ImplementProviderAttribute, IMessagePackHelper>(assemblies[i]);
                if (helper != null)
                {
                    Utility.MessagePack.SetHelper(helper);
                    break;
                }
            }
            GameManager.PreparatoryModule();
            AssignManager();
        }
        public void Run()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    if (!IsPause)
                        GameManager.OnRefresh();
                }
                catch (System.Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
        }
        public void OnPause()
        {
            GameManager.OnPause();
        }
        public void OnUnPause()
        {
            GameManager.OnUnPause();
        }
        void AssignManager()
        {
            try { FSMManager = GameManager.GetModule<IFSMManager>(); }
            catch (Exception e) { Utility.Debug.LogError(e); }
            try { ConfigManager = GameManager.GetModule<IConfigManager>(); }
            catch (Exception e) { Utility.Debug.LogError(e); }
            try { NetworkManager = GameManager.GetModule<INetworkManager>(); }
            catch (Exception e) { Utility.Debug.LogError(e); }
            try { ReferencePoolManager = GameManager.GetModule<IReferencePoolManager>(); }
            catch (Exception e) { Utility.Debug.LogError(e); }
            try { EventManager = GameManager.GetModule<IEventManager>(); }
            catch (Exception e) { Utility.Debug.LogError(e); }
        }
    }
}