using System;
using System.Data;

namespace UnitODB
{
	public abstract class OdbProvider : IOdbProvider
	{	
		public abstract string CreateColumn(OdbColumn col);
		public abstract IDbConnection CreateConnection();
		public abstract IDbContext CreateContext();   
		public abstract string SqlTypeStr(Type type);
		 
		public virtual string CreateTable(string name, string[] cols)
		{
			string sqlTable = "CREATE TABLE IF NOT EXISTS [" + name + "] ";

			sqlTable += "(\r\n" + string.Join(",\r\n", cols) + "\r\n);";

			return sqlTable;
		}

		public virtual string DropTable(string name)
		{
			return "DROP TABLE IF EXISTS [" + name + "]";
		} 
	}
}
