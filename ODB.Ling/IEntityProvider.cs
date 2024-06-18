using System.Linq;
using System.Linq.Expressions;

namespace UnitODB.Linq
{
	public interface IEntityProvider : IQueryProvider, IProvider
	{
		string Translate(Expression expression); 
 
	}
}