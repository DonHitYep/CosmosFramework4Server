using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public class ModuleBase<TDerived> : ConcurrentSingleton<TDerived>
        where TDerived : ModuleBase<TDerived>, new()
    {
    }
}
