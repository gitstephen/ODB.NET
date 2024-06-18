using System;
using System.Collections.Generic;

namespace UnitODB.Linq
{
	public static class TypeSystem
	{
		public static Type GetElementType(Type type)
		{
			Type type2 = FindIEnumerable(type);
			if (type2 == null)
			{
				return type;
			}
			return type2.GetGenericArguments()[0];
		}

		private static Type FindIEnumerable(Type type)
		{
			if (type == null || type == typeof(string))
			{
				return null;
			}

			if (type.IsArray)
			{
				return typeof(IEnumerable<>).MakeGenericType(type.GetElementType());
			}

			if (type.IsGenericType)
			{
				Type[] genericArguments = type.GetGenericArguments();
				foreach (Type type2 in genericArguments)
				{
					Type type3 = typeof(IEnumerable<>).MakeGenericType(type2);
					if (type3.IsAssignableFrom(type))
					{
						return type3;
					}
				}
			}

			Type[] interfaces = type.GetInterfaces();

			if (interfaces != null && interfaces.Length != 0)
			{
				Type[] genericArguments = interfaces;
				for (int i = 0; i < genericArguments.Length; i++)
				{
					Type type4 = FindIEnumerable(genericArguments[i]);
					if (type4 != null)
					{
						return type4;
					}
				}
			}

			if (type.BaseType != null && type.BaseType != typeof(object))
			{
				return FindIEnumerable(type.BaseType);
			}

			return null;
		}
	}
}