using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnitODB
{

	public class OdbMapping
	{
		public static OdbTable CreateTable(Type type)
		{
			OdbTable odbTable = new OdbTable(type);
			foreach (OdbColumn column in GetColumns(type))
			{
				if (column.Attribute.IsPrimaryKey)
				{
					odbTable.PK = column.Name;
				}
				odbTable.Columns.Add(column);
			}
			return odbTable;
		}

		public static string GetTableName(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(TableAttribute), inherit: false);
			if (customAttributes.Length != 0)
			{
				return (customAttributes[0] as TableAttribute).Name;
			}
			return type.Name;
		}

		public static ColumnAttribute GetColAttribute(PropertyInfo ptyInfo)
		{
			object[] customAttributes = ptyInfo.GetCustomAttributes(typeof(ColumnAttribute), inherit: true);
			ColumnAttribute columnAttribute = ((customAttributes.Length == 0) ? new ColumnAttribute() : (customAttributes[0] as ColumnAttribute));
			if (!IsGenericList(ptyInfo.PropertyType))
			{
				if (ptyInfo.PropertyType.IsClass)
				{
					if (ptyInfo.PropertyType != OdbType.String)
					{
						columnAttribute.IsForeignKey = true;
						if (string.IsNullOrEmpty(columnAttribute.Name))
						{
							columnAttribute.Name = ptyInfo.Name + "Id";
						}
					}
					else
					{
						columnAttribute.Length = 255;
					}
				}
			}
			else
			{
				columnAttribute.IsList = true;
			}
			if (ptyInfo.Name == "Id")
			{
				columnAttribute.IsPrimaryKey = true;
				columnAttribute.IsAuto = true;
				columnAttribute.IsNullable = false;
			}
			return columnAttribute;
		}

		public static bool IsGenericList(Type type)
		{
			if (type.IsGenericType)
			{
				return type.GetGenericTypeDefinition() == typeof(IList<>);
			}
			return false;
		}

		public static IEnumerable<OdbColumn> GetColumns(Type type)
		{
			PropertyInfo[] propes = type.GetProperties();
			foreach (PropertyInfo propertyInfo in propes)
			{
				ColumnAttribute colAttribute = GetColAttribute(propertyInfo);
				if (!colAttribute.NotMapped)
				{
					yield return new OdbColumn(propertyInfo, colAttribute);
				}
			}
		}
	}
}
