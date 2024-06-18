using System;
using System.Collections.Generic;
using System.Data;

namespace UnitODB
{
	public abstract class OdbContext : IDbContext, IDisposable
	{
		public IOdbProvider Provider { get; set; }
        protected IDbConnection Connection { get; set; }
		public IDbTransaction Transaction { get; set; }

		public int Depth { get; set; }
		
		private bool disposed;
		
		public OdbContext(IOdbProvider provider)
		{
			Provider = provider;

            this.Connection = Provider.CreateConnection();

			Depth = 1;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				if (Transaction != null)
				{
					Transaction.Dispose();
				}
				if (Connection != null)
				{
					if (Connection.State == ConnectionState.Open)
					{
						Connection.Close();
					}
					Connection.Dispose();
				}
			}

			Transaction = null;
			Connection = null;

			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual void BeginTrans()
		{
			if (Connection.State != ConnectionState.Open)
			{
				Connection.Open();
			}

			Transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
		}

		public virtual void SaveChanges()
		{
			if (Transaction == null)
			{
				throw new NullReferenceException("Transaction doesn't open.");
			}

			Transaction.Commit();
			Close();
		}

		public virtual void Cancel()
		{
			if (Transaction == null)
			{
				throw new NullReferenceException("Transaction doesn't open.");
			}

			Transaction.Rollback();
			Close();
		}

		public void Close()
		{
			if (this.Transaction != null)
			{
				Transaction.Dispose();
				Transaction = null;
			}
		}

		public abstract IQuery BuildQuery<T>(); 

        public virtual IQuery Select<T>(string[] cols)
		{
			return BuildQuery<T>().Select(cols).From();
		}

		public virtual IQuery Update<T>()
		{
			return BuildQuery<T>().Update();
		}

		public virtual IQuery Delete<T>()
		{
			return BuildQuery<T>().Delete();
		}

		public virtual IQuery Count<T>()
		{
			return BuildQuery<T>().Count("*").From();
		}  
 
		public static IList<T> Collection<T>(IEnumerable<T> enumerator)
		{
			IList<T> list = new List<T>();

			foreach (T item in enumerator)
			{
				list.Add(item);
			}

			return list;
		}

		public abstract int GetLastId();

		/// <summary>This method is get table all rows
		/// </summary>
		public virtual IList<T> ExecuteList<T>(IQuery q)
		{
			using (GenericEnumerator<T> enumerator = new GenericEnumerator<T>(q))
			{
				return Collection(enumerator);
			}
		}

		/// <summary>This method is get a row
		/// </summary>
		public virtual T ExecuteSingle<T>(IQuery q)
		{
			IList<T> list = ExecuteList<T>(q);

			if (list.Count == 0)
			{
				return default(T);
			}

			return list[0];
		}

		/// <summary>This method is persisted a object use nosql operation
		/// </summary>
		public virtual int ExecuteStore<T>(T t)
		{
			return new OdbWriter(this).Write(t as IEntity);
		}

		/// <summary>This method is insert array into table by column 
		/// </summary>
		public virtual int ExecuteInsert<T>(T t)
		{
			Type type = t.GetType();
			IQuery query = BuildQuery<T>();
			query.Table = OdbMapping.GetTableName(type);

			List<string> colNames = new List<string>();
			List<string> values = new List<string>();

			foreach (OdbColumn column in OdbMapping.GetColumns(type))
			{
				ColumnAttribute attribute = column.Attribute;

				if (!attribute.IsAuto && !attribute.IsForeignKey)
				{
					object value = column.GetValue(t);
					string item = query.SetParams(value);

					values.Add(item);
					colNames.Add("[" + column.Name + "]");
				}
			}

			query.Insert(colNames.ToArray()).Values(values.ToArray());
			return query.Execute();
		}

		/// <summary>This method is udpate table by column-values 
		/// </summary>
		public virtual int ExecuteUpdate<T>(T t)
		{
			Type type = t.GetType();
			IQuery query = BuildQuery<T>();
			query.Table = OdbMapping.GetTableName(type);

			List<string> cols = new List<string>();
			string key = "";
			object val = 0;

			foreach (OdbColumn column in OdbMapping.GetColumns(type))
			{
				ColumnAttribute attribute = column.Attribute;
				object value = column.GetValue(t);

				if (!attribute.IsPrimaryKey && !attribute.IsForeignKey)
				{
					string item = query.SetParams(value);
					cols.Add("[" + column.Name + "]=" + item);
				}
				else
				{
					key = column.Name;
					val = value;
				}
			}

			if (!string.IsNullOrEmpty(key))
			{
				query.Update().Set(cols.ToArray()).Where(key).Eq(val);
				return query.Execute();
			}

			throw new Exception("No key column");
		}

		/// <summary>This method is delete a row 
		/// </summary>
		public virtual int ExecuteDelete<T>(T t)
		{
			Type type = t.GetType();
			IQuery query = BuildQuery<T>();
			query.Table = OdbMapping.GetTableName(type);

			string col = "";
			object val = 0;

			foreach (OdbColumn column in OdbMapping.GetColumns(type))
			{
				if (column.Attribute.IsPrimaryKey)
				{
					col = column.Name;
					val = column.GetValue(t);
					break;
				}
			}

			if (!string.IsNullOrEmpty(col))
			{
				query.Delete().Where(col).Eq(val);
				return query.Execute();
			}

			throw new Exception("No key column");
		}

		/// <summary>This method is get one value
		/// </summary>
		public virtual T ExecuteScalar<T>(IQuery q)
		{
			return ExecuteScalar<T>(q.ToString(), q.GetParams()); 
		}

		/// <summary>This method is get one value
		/// </summary>
		public T ExecuteScalar<T>(string sql, params IDbDataParameter[] cmdParms)
		{
			using (IDbCommand dbCommand = SetCommand(Connection, Transaction, sql, cmdParms))
			{
				T result = (T)dbCommand.ExecuteScalar();

				dbCommand.Parameters.Clear();

				return result;
			}
		}

		/// <summary>This method is run a sql no return
		/// </summary>
		public virtual int ExecuteNonQuery(IQuery q)
		{
			return ExecuteNonQuery(q.ToString(), q.GetParams());
		}

		public int ExecuteNonQuery(string sql, params IDbDataParameter[] cmdParms)
		{
			using (IDbCommand dbCommand = SetCommand(Connection, Transaction, sql, cmdParms))
			{
				int result = dbCommand.ExecuteNonQuery();
				dbCommand.Parameters.Clear();

				return result;
			}
		}

		public int ExecuteReturnId(IQuery q)
		{
			ExecuteNonQuery(q);

			return GetLastId();
        } 

		protected static IDbCommand SetCommand(IDbConnection conn, IDbTransaction trans, string cmdText, IDbDataParameter[] cmdParms)
		{
			if (conn.State != ConnectionState.Open)
			{
				conn.Open();
			}

			IDbCommand dbCommand = conn.CreateCommand();
			dbCommand.Connection = conn;
			dbCommand.CommandText = cmdText;

			if (trans != null)
			{
				dbCommand.Transaction = trans;
			}

			dbCommand.CommandType = CommandType.Text;
			if (cmdParms != null)
			{
				foreach (IDbDataParameter value in cmdParms)
				{
					dbCommand.Parameters.Add(value);
				}
			}

			return dbCommand;
		}

		public IDataReader ExecuteReader(IQuery q)
		{
			return ExecuteReader(q.ToString(), q.GetParams());
		}

		public IDataReader ExecuteReader(string sql, params IDbDataParameter[] cmdParms)
		{
			using (IDbCommand dbCommand = SetCommand(Connection, Transaction, sql, cmdParms))
			{
				IDataReader result = dbCommand.ExecuteReader();
				dbCommand.Parameters.Clear();

				return result;
			}
		}

		public abstract IDbDataParameter CreateParameter(int n, object b);
	}
}
