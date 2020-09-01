using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public class Component:Object
    {
        Component parent;
        public Component Parent { get { return parent; } set { parent = value; } }
    }
}
