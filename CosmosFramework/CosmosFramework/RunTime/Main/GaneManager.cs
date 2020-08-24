﻿using Cosmos.Log;
using Cosmos.Network;
using Cosmos.Polling;
using Cosmos.Reference;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
   public class GameManager:ConcurrentSingleton<GameManager>
    {
        int moduleCount = 0;
        static Dictionary<ModuleEnum, IModule> moduleDict;
        internal static Dictionary<ModuleEnum, IModule> ModuleDict { get { return moduleDict; } }
        static ReferencePoolManager referencePoolManager;
        public  static ReferencePoolManager ReferencePoolManager
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
        static PollingManager  pollingManager;
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
        static LogManager  logManager;
        public static LogManager LogManager
        {
            get
            {
                if (logManager == null)
                {
                    logManager = new LogManager();
                    Instance.ModuleInitialization(logManager);
                }
                return logManager ;
            }
        }
        /// <summary>
        /// 构造函数，只有使用到时候才产生
        /// </summary>
        public GameManager()
        {
            if (moduleDict == null)
            {
                moduleDict = new Dictionary<ModuleEnum, IModule>();
            }
        }
        internal void ModuleInitialization(IModule module)
        {
            module.OnInitialization();
            Instance.RegisterModule(module.ModuleEnum, module);
        }
        /// <summary>
        /// 注册模块
        /// </summary>
        internal void RegisterModule(ModuleEnum moduleEnum, IModule module)
        {
            if (!HasModule(moduleEnum))
            {
                moduleDict.Add(moduleEnum, module);
                moduleCount++;
                Utility.Debug.LogInfo("Module:\"" + moduleEnum.ToString() + "\" " + "  is OnInitialization" + " based on GameManager");
            }
            else
            {
               Utility.Debug.LogError( new ArgumentException("Module:\"" + moduleEnum.ToString() + "\" " + " is already exist!")); 
            }
        }
        internal bool HasModule(ModuleEnum module)
        {
            return moduleDict.ContainsKey(module);
        }
    }
}
