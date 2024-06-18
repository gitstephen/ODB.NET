using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnitODB.Linq
{
	public interface IEntityQuery<T> : IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T>, IEnumerable, IQueryable, IOrderedQueryable
	{
		string GetSQL();
	}
}
