using System;
using System.Data;

namespace UnitODB
{
	public abstract class OdbProvider : IOdbProvider
	{
        public string DB { get; set; }
        public IDbContext DbContext { get; set; }
                
		protected OdbProvider(string db)
        {
            if (string.IsNullOrEmpty(db))
            {
                throw new ArgumentNullException("db", "Database path cannot be null or empty.");
            }

            this.DB = db;
        } 
 
        public abstract string CreateColumn(OdbColumn col);
		public abstract IDbConnection CreateConnection();
		public abstract IDbContext CreateContext();
		public abstract IDbDataParameter CreateParameter(int index, object value);
        
    }
}
