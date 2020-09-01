using System.Collections;
using System;
namespace Cosmos
{
    public interface IModule: IControllableBehaviour
    {
        string ModuleFullyQualifiedName { get; }
        ModuleEnum ModuleEnum { get; }
    }
}
