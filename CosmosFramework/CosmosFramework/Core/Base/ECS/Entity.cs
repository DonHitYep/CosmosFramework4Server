using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public class Entity:ComponentWithId
    {
        Dictionary<GenericValuePair<Type, byte>, Component> compDict = new Dictionary<GenericValuePair<Type, byte>, Component>();
    }
}
