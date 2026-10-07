using System;
using System.Data;
using System.Data.SQLite;
using System.Linq.Expressions;
using System.Reflection;
using UnitODB;
using UnitODB.Linq;

namespace UnitODB.SQLite
{
    public class SQLiteOdbProvider : OdbEntityProvider, IProvider
    {
		public string DB { get; set; }

		public SQLiteOdbProvider(string db)
		{
			this.DB = db;
		} 

        public override IDbContext CreateContext()
		{  
			if (this.DbContext == null)
			{
				this.DbContext = new SQLiteOdbContext(this);
            }

			return this.DbContext; 
        }

		public override IDbConnection CreateConnection()
		{
			return new SQLiteConnection(DB);
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

        public override IDbDataParameter CreateParameter(int index, object value)
        {
            return new SQLiteParameter
            {
                ParameterName = "@p" + index,
                Value = value ?? DBNull.Value,
                DbType = OdbSqlType.Get(value)
            };
        }

        public override string ToSql(Expression expression)
        {
            if (this.Visitor == null)
            {
                this.Visitor = new SQLiteVisitor(this, 2);
            }

            return this.Visitor.Translate(expression);
        }

        public override IEntityQuery<T> CreateQuery<T>()
        {
            return new EntityQuery<T>(this);
        }
    }
}
