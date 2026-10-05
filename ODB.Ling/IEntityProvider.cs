using System.Linq;
using System.Linq.Expressions;

namespace UnitODB.Linq
{
	public interface IEntityProvider : IQueryProvider, IProvider
	{
        IEntityQuery<T> CreateQuery<T>() where T : IEntity;
        string Translate(Expression expression);
    }
}