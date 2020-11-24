using System;
using System.Collections;
using System.Collections.Generic;
namespace Cosmos.Mvvm
{
    public static class MVVM
    {
        static Dictionary<TypeStringPair, View> viewDict = new Dictionary<TypeStringPair, View>();
        static Dictionary<TypeStringPair, Model> modelDict = new Dictionary<TypeStringPair, Model>();
        static Dictionary<string, ViewModel> viewModelDict = new Dictionary<string, ViewModel>();
        public static void RegisterView<T>(T view)
            where T : View
        {
            view.RegisterEvent();
            var typeString = new TypeStringPair(typeof(T), view.Name);
            viewDict.AddOrUpdate(typeString, view);
        }
        public static void RegisterModel<T>(T model)
            where T : Model
        {
            var typeString = new TypeStringPair(typeof(T), model.Name);
            modelDict.AddOrUpdate(typeString, model);
        }
        public static void RegisterViewModel<T>( string viewModelName)
where T : ViewModel
        {
            var viewModel= Utility.Assembly.GetTypeInstance(typeof(T)) as ViewModel;
            viewModelDict.AddOrUpdate(viewModelName, viewModel);
        }
        public static void RegisterViewModel(Type viewModelType, string viewModelName)
        {
            if (typeof(ViewModel).IsAssignableFrom(viewModelType))
            {
                var viewModel = Utility.Assembly.GetTypeInstance(viewModelType) as ViewModel;
                viewModelDict.AddOrUpdate(viewModelName, viewModel);
            }
            else
            {
                throw new ArgumentException($"viewModel :{viewModelType} is not inherit form ViewModel !");
            }
        }
        public static T GetView<T>() where T : View
        {
            var typeString = new TypeStringPair(typeof(T));
            var result = viewDict.TryGetValue(typeString, out var view);
            if (result)
                return view as T;
            else
                return null;
        }
        public static T GetView<T>(string viewName) where T : View
        {
            var typeString = new TypeStringPair(typeof(T), viewName);
            var result = viewDict.TryGetValue(typeString, out var view);
            if (result)
                return view as T;
            else
                return null;
        }
        public static T GetModel<T>() where T : Model
        {
            var typeString = new TypeStringPair(typeof(T));
            var result = modelDict.TryGetValue(typeString, out var model);
            if (result)
                return model as T;
            else
                return null;
        }
        public static T GetModel<T>(string modelName) where T : Model
        {
            var typeString = new TypeStringPair(typeof(T), modelName);
            var result = modelDict.TryGetValue(typeString, out var model);
            if (result)
                return model as T;
            else
                return null;
        }
        public static T GetViewModel<T>(string viewModelName) where T : ViewModel
        {
            var result = viewModelDict.TryGetValue(viewModelName, out var vm);
            if (result)
                return vm as T;
            else
                return null;
        }
        public static void SendEvent(string eventName, object data = null)
        {
            if( viewModelDict.TryGetValue(eventName, out var viewModel))
            {
                viewModel.Execute(data);
            }
            foreach (var view in viewDict.Values)
            {
                view.ExecuteEvent(eventName, data);
            }
        }
    }
}
