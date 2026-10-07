using System;

namespace UnitODB
{
	[Serializable]
	public class OdbException : Exception
	{
		public OdbException(string message)
			: base(message)
		{
		}
	}
}
