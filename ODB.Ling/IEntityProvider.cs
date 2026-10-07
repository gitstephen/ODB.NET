using System.Linq;
using System.Linq.Expressions;

namespace UnitODB.Linq
{
	public interface IEntityProvider : IQueryProvider, IOdbProvider
    {
        IEntityQuery<T> CreateQuery<T>() where T : IEntity;
        string ToSql(Expression expression);
    }
}