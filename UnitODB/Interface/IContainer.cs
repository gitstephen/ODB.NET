using System;

namespace UnitODB
{
	public interface IContainer : IDisposable
	{ 
		IDbContext Context { get; }
		IOdbProvider Provider { get; set; }

		bool AutoCommit { get; set; } 

		void Clear<T>();

		IQuery Query<T>(string[] cols); 
 
		void RegisterAdd<T>(T t);

		void RegisterUpdate<T>(T t);

		void RegisterDelete<T>(T t);

		void RegisterTask(Action action);

		void Commit();
	}
}
