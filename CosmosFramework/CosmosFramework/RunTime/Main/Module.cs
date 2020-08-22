using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// Server端的模块
    /// </summary>
    /// <typeparam name="TDerived"></typeparam>
    public class Module<TDerived> : ConcurrentSingleton<TDerived>
        where TDerived : Module<TDerived>, new()
    {
    }
}
