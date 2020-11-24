using System.Collections;
using System.Collections.Generic;
namespace Cosmos.Mvvm
{
    public abstract class ViewModel
    {
        public abstract void Execute( object data);
        protected T GetModel<T>() where T : Model
        {
            return MVVM.GetModel<T>();
        }
        protected T GetModel<T>(string modelName) where T : Model
        {
            return MVVM.GetModel<T>(modelName);
        }
        protected T GetView<T>()where T :View
        {
            return MVVM.GetView<T>();
        }
        protected T GetView<T>(string viewName) where T : View
        {
            return MVVM.GetView<T>(viewName);
        }
        protected void RegisterView(View view)
        {
            MVVM.RegisterView(view);
        }
        protected void RegisterModel(Model model)
        {
            MVVM.RegisterModel(model);
        }
        protected void RegisterViewModel<T>(string viewModelName)
            where T :ViewModel
        {
            MVVM.RegisterViewModel<T>(viewModelName);
        }
    }
}