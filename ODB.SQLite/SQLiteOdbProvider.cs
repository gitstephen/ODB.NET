using System;
using System.Data;
using System.Data.SQLite;
using UnitODB;

namespace UnitODB.SQLite
{
    public class SQLiteOdbProvider : OdbProvider, IProvider
    {
		public string DbString { get; set; }

		public SQLiteOdbProvider(string db)
		{
			this.DbString = db;
		} 

        public override IDbContext CreateContext()
		{  
			return new SQLiteOdbContext(this);
		}

		public override IDbConnection CreateConnection()
		{
			return new SQLiteConnection(DbString);
		}

		public override string CreateColumn(OdbColumn col)
		{
			string typeStr = SqlTypeStr(col.GetMapType());
			string name = "[" + col.Name + "] ";

			ColumnAttribute attribute = col.Attribute;

			string stm = (!attribute.IsPrimaryKey) ? (name + typeStr) : (name + "INTEGER PRIMARY KEY");
			
			if (attribute.IsAuto)
			{
				stm += " AUTOINCREMENT";
			}
			if (attribute.IsNullable && !attribute.IsPrimaryKey)
			{
				return stm + " NULL";
			}
			
			return stm + " NOT NULL";
		}
		 
		public override string SqlTypeStr(Type type)
		{
			switch (OdbSqlType.Convert(type))
			{
				case DbType.String:
					return "TEXT";
				case DbType.StringFixedLength:
					return "CHAR(1)";
				case DbType.SByte:
					return "TINYINT";
				case DbType.Byte:
				case DbType.Int16:
					return "SMALLINT";
				case DbType.Int32:
				case DbType.UInt16:
					return "INT";
				case DbType.Int64:
				case DbType.UInt32:
					return "INTEGER";
				case DbType.Double:
					return "REAL";
				case DbType.Single:
					return "FLOAT";
				case DbType.Decimal:
					return "NUMERIC(20,10)";
				case DbType.Boolean:
					return "BOOLEAN";
				case DbType.DateTime:
					return "TIMESTAMP";
				case DbType.Binary:
					return "BLOB";
				case DbType.Guid:
					return "GUID";
				default:
					return "TEXT";
			}
		} 
	}
}
