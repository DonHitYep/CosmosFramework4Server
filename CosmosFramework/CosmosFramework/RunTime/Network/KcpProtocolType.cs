using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos
{
	/// <summary>
	/// KCP网络协议类型
	/// </summary>
	public static class KcpProtocalType
	{
		/// <summary>
		/// 无状态
		/// </summary>
		public const byte NIL = 0;
		/// <summary>
		/// 同步标志
		/// </summary>
		public const byte SYN = 1;
		/// <summary>
		/// 确认标志
		/// </summary>
		public const byte ACK = 2;
		/// <summary>
		/// 结束标志
		/// </summary>
		public const byte FIN = 3;
		/// <summary>
		///  业务标志
		/// </summary>
		public const byte MSG = 4;
	}
}

