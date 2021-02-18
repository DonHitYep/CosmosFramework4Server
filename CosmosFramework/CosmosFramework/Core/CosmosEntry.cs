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
    public class CosmosEntry 
    {
        public static bool IsPause { get; private set; }
        public static bool Pause
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
        public static IFSMManager FSMManager { get { return GameManager.GetModule<IFSMManager>(); } }
        public static IConfigManager ConfigManager { get { return GameManager.GetModule<IConfigManager>(); } }
        public static INetworkManager NetworkManager { get { return GameManager.GetModule<INetworkManager>(); } }
        public static IReferencePoolManager ReferencePoolManager { get { return GameManager.GetModule<IReferencePoolManager>(); } }
        public static IEventManager EventManager { get { return GameManager.GetModule<IEventManager>(); } }
        public static void LaunchHelpers()
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
        }
        public static void LaunchModules()
        {
            GameManager.PreparatoryModule();
        }
        public  static void Run()
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
        public static void OnPause()
        {
            GameManager.OnPause();
        }
        public static void OnUnPause()
        {
            GameManager.OnUnPause();
        }
    }
}