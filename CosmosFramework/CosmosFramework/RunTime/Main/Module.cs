using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// Server端的模块
    /// </summary>
    /// <typeparam name="TDerived"></typeparam>
    public abstract class Module<TDerived> : ConcurrentSingleton<TDerived>, IModule,IOperable
        where TDerived : Module<TDerived>, new()
    {
        #region properties
        /// <summary>
        /// 模块的非完全限定名 
        /// </summary>
        string moduleName = null;
        public string ModuleName
        {
            get
            {
                if (string.IsNullOrEmpty(moduleName))
                    moduleName = Utility.Text.StringSplit(Utility.Assembly.GetTypeFullName<TDerived>(), new string[] { "." }, true, 2);
                return moduleName;
            }
        }
        /// <summary>
        /// 模块的完全限定名
        /// </summary>
        string moduleFullyQualifiedName = null;
        public string ModuleFullyQualifiedName
        {
            get
            {
                if (string.IsNullOrEmpty(moduleFullyQualifiedName))
                    moduleFullyQualifiedName = Utility.Assembly.GetTypeFullName<TDerived>();
                return moduleFullyQualifiedName;
            }
        }
        public ModuleEnum ModuleEnum
        {
            get
            {
                var module = ModuleName.Replace("Manager", "");
                return Utility.Framework.GetModuleEnum(module);
            }
        }
        public bool IsPause { get; protected set; }
        #endregion

        #region Methods
        /// <summary>
        /// 空虚函数
        /// </summary>
        public virtual void OnInitialization(){}
        public void OnPause(){IsPause = false;}
        /// <summary>
        /// 空虚函数
        /// </summary>
        public virtual void OnPreparatory(){}
        /// <summary>
        /// 空虚函数
        /// </summary>
        public virtual void OnRefresh(){}
        /// <summary>
        /// 空虚函数
        /// </summary>
        public virtual void OnTermination(){}
        public void OnUnPause(){IsPause = false;}
        public virtual void OnActive(){}
        public virtual void OnDeactive(){}
        #endregion
    }
}
