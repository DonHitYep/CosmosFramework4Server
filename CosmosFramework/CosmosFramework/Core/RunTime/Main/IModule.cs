using System.Collections;
using System;
namespace Cosmos
{
    public interface IModule: IControllableBehaviour
    {
        Type ModuleType { get; }
    }
}
