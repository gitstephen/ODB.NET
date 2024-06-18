using System.Collections;
using System.Collections.Generic;

namespace UnitODB
{
	public interface IRepository
	{
		IContainer Unit { get; }
	}

	public interface IRepository<T> : IRepository, IEnumerable<T>, IEnumerable
	{
		T Get(int id);

		void Add(T t);

		void Update(T t);

		void Delete(T t);

		long Count();

		IList<T> ToList();

		void SetDepth(int n);
	}
}
