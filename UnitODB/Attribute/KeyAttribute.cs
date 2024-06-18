using System;

namespace UnitODB
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public class KeyAttribute : ColumnAttribute
	{
		public KeyAttribute(string name = "")
			: base(name)
		{
			base.IsAuto = true;
			base.IsNullable = false;
			base.IsPrimaryKey = true;
		}
	}
}
