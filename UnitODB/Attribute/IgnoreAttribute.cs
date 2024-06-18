using System;

namespace UnitODB
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
				Inherited = false, AllowMultiple = false)]
	public class IgnoreAttribute : ColumnAttribute
	{
		public IgnoreAttribute()
		{
			base.NotMapped = true;
		}
	}
}
