﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
    public sealed partial class GameManager
    {
        /// <summary>
        /// 外源模块容器；
        /// </summary>
        static Dictionary<Type, IModule> outerModuleDict = new Dictionary<Type, IModule>();
        /// <summary>
        /// 线程安全；
        /// 获取外源模块；
        /// 需要从Module类派生;
        /// 此类模块不由CF框架生成，由用户自定义
        /// </summary>
        /// <typeparam name="TModule">实现模块功能的类对象</typeparam>
        /// <returns>获取的模块</returns>
        public static TModule OuterModule<TModule>()
            where TModule : Module<TModule>, new()
        {
            Type type = typeof(TModule);
            IModule module = default;
            var result = outerModuleDict.TryGetValue(type, out module);
            if (result)
            {
                return module as TModule;
            }
            else
                return default(TModule);
        }
        public static void InitOuterModule(Type type)
        {
            Type[] types = Assembly.GetAssembly(type).GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].GetCustomAttribute<OuterModuleAttribute>() != null)
                {
                    var module = Utility.Assembly.GetTypeInstance(types[i]) as IModule;
                    var result = outerModuleDict.TryAdd(types[i], module);
                    if (result)
                    {
                        module.OnInitialization();
                        Utility.Debug.LogInfo($"Module :{module.ToString()} instanced  ");
                        GameManager.Instance.refreshHandler += module.OnRefresh;
                    }
                }
            }
            PrepareOuterModule();
        }
        static void PrepareOuterModule()
        {
            foreach (var module in outerModuleDict.Values)
            {
                module.OnPreparatory();
            }
        }
    }
}
