using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public class ComponentWithId : Component
    {
		public long Id { get; set; }
		protected ComponentWithId()
		{
			this.Id = this.InstanceId;
		}
		protected ComponentWithId(long id)
		{
			this.Id = id;
		}

	}
}
