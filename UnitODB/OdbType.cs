using System;

namespace UnitODB
{

	public class OdbType
	{
		public static readonly Type Int32 = typeof(int);

		public static readonly Type UInt32 = typeof(uint);

		public static readonly Type Int64 = typeof(long);

		public static readonly Type UInt64 = typeof(ulong);

		public static readonly Type Byte = typeof(byte);

		public static readonly Type Bytes = typeof(byte[]);

		public static readonly Type SByte = typeof(sbyte);

		public static readonly Type Short = typeof(short);

		public static readonly Type UShort = typeof(ushort);

		public static readonly Type Decimal = typeof(decimal);

		public static readonly Type Single = typeof(float);

		public static readonly Type Double = typeof(double);

		public static readonly Type String = typeof(string);

		public static readonly Type Char = typeof(char);

		public static readonly Type Bool = typeof(bool);

		public static readonly Type DateTime = typeof(DateTime);

		public static readonly Type NullableDateTime = typeof(DateTime?);

		public static readonly Type Guid = typeof(Guid);

		public static readonly Type OdbEntity = typeof(IEntity);

		public static bool IsNullable(Type type)
		{
			if (type.IsGenericType)
			{
				return type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			return false;
		}
	}
}
