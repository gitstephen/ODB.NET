using System;
using System.Data;
using System.Data.SQLite;
using UnitODB;

namespace UnitODB.SQLite
{
    public class SQLiteOdbContext : OdbContext
    { 
		public SQLiteOdbContext(IOdbProvider provider)
			: base(provider)
		{
		}

		public override int GetLastId()
		{ 
			return (int)(this.Connection as SQLiteConnection).LastInsertRowId;
		} 

        public override IQuery BuildQuery<T>()
        {
            return new SQLiteQuery<T>(this);
        } 
    }
}
