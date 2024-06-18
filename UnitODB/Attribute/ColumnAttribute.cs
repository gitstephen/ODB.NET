using System;

namespace UnitODB
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public class ColumnAttribute : Attribute
	{
		public string Name { get; set; }

		public int Length { get; set; }

		public bool IsAuto { get; set; }

		public bool IsNullable { get; set; }

		public bool NotMapped { get; set; }

		public bool IsPrimaryKey { get; set; }

		public bool IsForeignKey { get; set; }

		public bool IsList { get; set; }

		public ColumnAttribute()
			: this("")
		{
		}

		public ColumnAttribute(string name)
			: this(name, 0)
		{
		}

		public ColumnAttribute(string name, int size)
		{
			Name = name;
			Length = size;
			IsAuto = false;
			IsNullable = true;
			NotMapped = false;
			IsPrimaryKey = false;
			IsForeignKey = false;
			IsList = false;
		}
	}
}
