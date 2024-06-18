using System;

namespace UnitODB
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public class LengthAttribute : ColumnAttribute
	{
		public LengthAttribute(int size = 255)
		{
			base.Length = size;
		}
	}
}
