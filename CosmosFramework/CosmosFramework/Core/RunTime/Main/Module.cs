using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 模块的抽象基类；
    /// 外部可扩展；
    /// </summary>
    public abstract class Module : IControllableBehaviour, IOperable, IElapesRefreshable
    {
        #region properties
        public bool IsPause { get; protected set; }
        #endregion

        #region Methods
        /// <summary>
        /// 空虚函数;
        /// </summary>
        public virtual void OnInitialization() { }
        /// <summary>
        /// 空虚函数，停止模块
        /// </summary>
        public virtual void OnTermination() { }
        public void OnPause() { IsPause = false; }
        /// <summary>
        /// 空虚函数
        /// </summary>
        public virtual void OnPreparatory() { }
        /// <summary>
        /// 空虚函数
        /// </summary>
        public virtual void OnRefresh() { }
        /// <summary>
        /// 时间流逝轮询;
        /// </summary>
        /// <param name="msNow">utc毫秒当前时间</param>
        public virtual void OnElapseRefresh(long msNow) { if (IsPause) return; }
        public void OnUnPause() { IsPause = false; }
        public virtual void OnActive() { }
        public virtual void OnDeactive() { }
        #endregion
    }
}
