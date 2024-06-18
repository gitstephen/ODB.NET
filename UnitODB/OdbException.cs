using System;

namespace UnitODB
{
	public class OdbException : Exception
	{
		public OdbException(string message)
			: base(message)
		{
		}
	}
}
