using System;

namespace UnitODB
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
	public class TableAttribute : Attribute
	{
		public string Name { get; set; }

		public string Schema { get; set; }

		public TableAttribute()
			: this("")
		{
		}

		public TableAttribute(string name)
			: this(name, "")
		{
		}

		public TableAttribute(string name, string schema)
		{
			Name = name;
			Schema = schema;
		}
	}
}
