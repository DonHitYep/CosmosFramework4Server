using System.Collections;
using System.Collections.Generic;
namespace Cosmos.Mvvm
{
    public abstract class View 
    {
        public abstract string Name { get; }
        protected List<string> eventNameList = new List<string>();
        public void ExecuteEvent(string eventName, object data)
        {
            if (eventNameList.Contains(eventName))
            {
                HandleEvent(eventName, data);
            }
        }
        /// <summary>
        /// 注册事件
        /// </summary>
        public virtual void RegisterEvent() { }
        protected abstract void HandleEvent(string eventName, object data = null);
        protected T GetViewModel<T>(string viewModelName)
            where T : ViewModel
        {
            return MVVM.GetViewModel<T>(viewModelName);
        }
        protected void SendEvent(string eventName, object data = null)
        {
            MVVM.SendEvent(eventName, data);
        }
    }
}