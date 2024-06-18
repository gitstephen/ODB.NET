using System;
using System.Reflection;

namespace UnitODB
{
	public class OdbColumn
	{
		public string Name { get; set; }

		public PropertyInfo Property { get; private set; }

		public ColumnAttribute Attribute { get; private set; }

		public OdbColumn(PropertyInfo property, ColumnAttribute attr)
		{
			Property = property;
			Attribute = attr;
			Name = (string.IsNullOrEmpty(attr.Name) ? property.Name : attr.Name);
		}

		public Type GetMapType()
		{
			return Property.PropertyType;
		}

		public virtual object GetValue(object b)
		{
			object value = Property.GetValue(b, null);
			if (Property.PropertyType.IsEnum)
			{
				return (int)value;
			}
			return value;
		}

		public virtual void SetValue(object instance, object value)
		{
			if (Attribute.IsPrimaryKey)
			{
				value = Convert.ToInt32(value);
			}
			if (Property.PropertyType.IsEnum)
			{
				value = Enum.ToObject(Property.PropertyType, value);
			}
			Property.SetValue(instance, value, null);
		}
	}
}
