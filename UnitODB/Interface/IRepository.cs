using System.Collections;
using System.Collections.Generic;

namespace UnitODB
{
	public interface IRepository
	{
        IDbContext DbContext { get; }
	}

	public interface IRepository<T> : IRepository, IEnumerable<T>, IEnumerable
	{
		T Get(int id);

		long Count();

		IList<T> ToList();

		void SetDepth(int n);
	}
}
