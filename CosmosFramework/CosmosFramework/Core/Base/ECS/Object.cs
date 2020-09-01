using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public abstract class Object : IBehaviour, IPreparatory
    {
        public virtual void OnInitialization(){}
        public virtual void OnPreparatory(){}
        public virtual void OnTermination(){}
    }
}
