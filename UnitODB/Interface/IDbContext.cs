using System;
using System.Collections.Generic;
using System.Data;

namespace UnitODB
{
	public interface IDbContext : IDisposable {

        IOdbProvider Provider { get; } 
		int Depth { get; set; }
				
		void BeginTrans();

		void SaveChanges();

		void Cancel();

		void Close();

        int GetLastId();

        IQuery BuildQuery<T>();

		IQuery Select<T>(string[] cols);

		IQuery Delete<T>();

		IQuery Update<T>();

		IQuery Count<T>(); 

		int ExecuteStore<T>(T t);

		int ExecuteInsert<T>(T t);

		int ExecuteUpdate<T>(T t);

		int ExecuteDelete<T>(T t);

		T ExecuteScalar<T>(IQuery q);
		
		int ExecuteNonQuery(IQuery q);

		int ExecuteReturnId(IQuery q);

		T ExecuteSingle<T>(IQuery q);

		IList<T> ExecuteList<T>(IQuery q);
 
		int ExecuteNonQuery(string sql, params IDbDataParameter[] cmdParms);

		T ExecuteScalar<T>(string sql, params IDbDataParameter[] cmdParms);

		IDataReader ExecuteReader(string sql, params IDbDataParameter[] cmdParms);

		IDataReader ExecuteReader(IQuery q);

		IDbDataParameter CreateParameter(int n, object b); 
    }
}
