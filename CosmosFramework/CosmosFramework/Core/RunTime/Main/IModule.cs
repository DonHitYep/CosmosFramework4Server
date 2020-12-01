using System.Collections;
using System;
namespace Cosmos
{
    public interface IModule: IControllableBehaviour, IOperable
    {
        Type ModuleType { get; }
    }
}
