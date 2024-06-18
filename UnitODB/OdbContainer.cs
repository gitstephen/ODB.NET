using System;
using System.Data;
using System.Collections.Generic;

namespace UnitODB
{
	public class OdbContainer : IContainer, IDisposable
	{ 
		public IDbContext Context { get; private set; }
		public IOdbProvider Provider { get; set; }
		public bool AutoCommit { get; set; }	
		
		private List<Action> actionRegisters;

		private bool disposed;

		public OdbContainer(IOdbProvider provider, int depth)
		{
			Provider = provider; 

			Context = provider.CreateContext();

			Context.Depth = depth;

			AutoCommit = true;
			
			actionRegisters = new List<Action>();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				if (Context != null)
				{
					Context.Dispose();
				}

				Cleanup();
			}

			Context = null;
			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public virtual IQuery Query<T>(string[] cols)
		{
			return Context.Select<T>(cols);
		}

		/// <summary>This method is create a table by column
		/// </summary>
		private int Create(Type type)
		{
			List<string> list = new List<string>();
			bool flag = false;

			foreach (OdbColumn column in OdbMapping.GetColumns(type))
			{
				if (!column.Attribute.IsList)
				{
					if (column.Attribute.IsForeignKey)
					{
						Create(column.GetMapType());
					}

					if (column.Attribute.IsPrimaryKey)
					{
						flag = true;
					}

					list.Add(Provider.CreateColumn(column));
				}
			}

			if (flag)
			{
				string tableName = OdbMapping.GetTableName(type);
				string sql = Provider.CreateTable(tableName, list.ToArray());

				return Context.ExecuteNonQuery(sql);
			}

			throw new Exception("No key column");
		}

		/// <summary>This method is create a table by object
		/// </summary>
		public virtual void Create<T>()
		{
			Create(typeof(T));
		}

		private int Drop(Type type)
		{
			foreach (OdbColumn column in OdbMapping.GetColumns(type))
			{
				if (column.Attribute.IsForeignKey)
				{
					Drop(column.GetMapType());
				}
			}

			string tableName = OdbMapping.GetTableName(type);
			string sql = Provider.DropTable(tableName);

			return Context.ExecuteNonQuery(sql);
		}

		public virtual void Drop<T>()
		{
			Drop(typeof(T));
		}

		public virtual void Clear<T>()
		{
			RegisterTask(delegate
			{
				Context.BuildQuery<T>().Delete().Execute();
			});
		} 

		public void RegisterAdd<T>(T t)
		{
			RegisterTask(delegate
			{
				Context.ExecuteStore(t);
			});
		}

		public void RegisterUpdate<T>(T t)
		{
			RegisterTask(delegate
			{
				Context.ExecuteStore(t);
			});
		}

		public void RegisterDelete<T>(T t)
		{
			RegisterTask(delegate
			{
				Context.ExecuteDelete(t);
			});
		}

		public void RegisterTask(Action action)
		{
			actionRegisters.Add(action);

			if (AutoCommit)
			{
				Commit();
			}
		}

		public virtual void Commit()
		{
			try
			{
				Context.BeginTrans();

				foreach (Action actionRegister in actionRegisters)
				{
					actionRegister();
				}

				Context.SaveChanges();
			}
			catch (Exception ex)
			{
				Context.Cancel();

				throw ex;
			}
			finally
			{
				Cleanup();
				AutoCommit = true;
			}
		}

		private void Cleanup()
		{
			if (actionRegisters.Count > 0)
			{
				actionRegisters.Clear();
			}
		}
	}
}
