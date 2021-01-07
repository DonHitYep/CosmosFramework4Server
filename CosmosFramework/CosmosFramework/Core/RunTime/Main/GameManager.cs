using Cosmos.Network;
using Cosmos.Reference;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public sealed partial class GameManager 
    {
        #region Properties
        /// <summary>
        /// 轮询更新委托
        /// </summary>
        internal static Action refreshHandler;
        public static event Action RefreshHandler
        {
            add { refreshHandler += value; }
            remove { refreshHandler -= value; }
        }
        /// <summary>
        /// 时间流逝轮询委托；
        /// </summary>
        static Action<long> elapseRefreshHandler;
        public static bool IsPause { get; private set; }
        //当前注册的模块总数
        static int moduleCount = 0;
        internal static int ModuleCount { get { return moduleCount; } }
        /// <summary>
        /// 模块字典；
        /// key=>moduleType；value=>module
        /// </summary>
        static ConcurrentDictionary<Type, Module> moduleDict;
        /// <summary>
        /// 接口-module字典；
        /// key=>IModuleManager；value=>Module
        /// </summary>
        static Dictionary<Type, Type> interfaceModuleDict;
        #endregion
        #region Methods
        /// <summary>
        /// 获取模块；
        /// 若需要进行外部扩展，请继承自Module，需要实现接口 IModuleManager，并标记特性：ModuleAttribute
        /// 如：public class TestManager:Module,ITestManager{}
        /// ITestManager 需要包含所有外部可调用的方法；具体请参考Cosmos源码；
        /// </summary>
        /// <typeparam name="T">内置模块接口</typeparam>
        /// <returns>模板模块接口</returns>
        public static T GetModule<T>() where T : class, IModuleManager
        {
            Type interfaceType = typeof(T);
            var hasType = interfaceModuleDict.TryGetValue(interfaceType, out var derivedType);
            if (!hasType)
            {
                foreach (var m in moduleDict)
                {
                    if (interfaceType.IsAssignableFrom(m.Key))
                    {
                        derivedType = m.Key;
                        interfaceModuleDict.TryAdd(interfaceType, derivedType);
                        break;
                    }
                }
            }
            moduleDict.TryGetValue(derivedType, out var module);
            return module as T;
        }
        public static void PreparatoryModule()
        {
            foreach (var module in moduleDict.Keys)
            {
                Utility.Debug.LogInfo($"Module :{module} has  been initialized");
            }
        }
        public static void Dispose()
        {
            OnDeactive();
        }
        /// <summary>
        /// 构造函数，只有使用到时候才产生
        /// </summary>
        static GameManager()
        {
            if (moduleDict == null)
            {
                moduleDict = new ConcurrentDictionary<Type, Module>();
                interfaceModuleDict = new Dictionary<Type, Type>();
            }
            InitModule();
        }
        internal static void OnPause()
        {
            IsPause = true;
        }
        internal static void OnUnPause()
        {
            IsPause = false;
        }
        internal static void OnRefresh()
        {
            if (IsPause)
                return;
            refreshHandler?.Invoke();
        }
        /// <summary>
        /// 时间流逝轮询;
        /// </summary>
        /// <param name="msNow">utc毫秒当前时间</param>
        internal static void OnElapseRefresh(long msNow)
        {
            if (IsPause)
                return;
            elapseRefreshHandler?.Invoke(msNow);
        }
        /// <summary>
        /// 终结并释放GameManager
        /// </summary>
        internal static bool HasModule(Type type)
        {
            return moduleDict.ContainsKey(type);
        }
        static void ModuleTermination(Module module)
        {
            var type = module.GetType();
            if (HasModule(type))
            {
                module.OnDeactive();
                var m = moduleDict[type];
                RefreshHandler -= module.OnRefresh;
                moduleDict.TryRemove(type,out _ );
                moduleCount--;
                module.OnTermination();
                Utility.Debug.LogInfo($"Module :{module} is OnTermination", MessageColor.DARKBLUE);
            }
            else
                throw new ArgumentException($"Module : {module} is not exist!");
        }
        static void InitModule()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyLength = assemblies.Length;
            for (int h = 0; h < assemblyLength; h++)
            {
                var modules = Utility.Assembly.GetInstancesByAttribute<ModuleAttribute, Module>(assemblies[h]);
                for (int i = 0; i < modules.Length; i++)
                {
                    var type = modules[i].GetType();
                    if (!HasModule(type))
                    {
                        if (moduleDict.TryAdd(type, modules[i]))
                        {
                            try
                            {
                                modules[i].OnInitialization();
                                moduleCount++;
                            }
                            catch (Exception e)
                            {
                                Utility.Debug.LogError(e);
                            }
                        }
                    }
                    else
                        throw new ArgumentException($"Module : {type} is already exist!");
                }
            }
            ActiveModule();
        }
        static void ActiveModule()
        {
            foreach (var module in moduleDict.Values)
            {
                try
                {
                    (module as Module).OnActive();
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
            PrepareModule();
        }
        static void PrepareModule()
        {
            foreach (var module in moduleDict.Values)
            {
                try
                {
                    module.OnPreparatory();
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
            AddRefreshListen();
        }
        static void AddRefreshListen()
        {
            foreach (var module in moduleDict.Values)
            {
                GameManager.RefreshHandler += module.OnRefresh;
            }
        }
        static void OnDeactive()
        {
            foreach (var module in moduleDict?.Values)
            {
                try
                {
                    module?.OnDeactive();
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
            OnTermination();
        }
        static void OnTermination()
        {
            foreach (var module in moduleDict?.Values)
            {
                try
                {
                    module?.OnTermination();
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
            RemoveRefreshListen();
        }
        static void RemoveRefreshListen()
        {
            foreach (var module in moduleDict.Values)
            {
                GameManager.RefreshHandler -= module.OnRefresh;
            }
        }
        #endregion
    }
}
