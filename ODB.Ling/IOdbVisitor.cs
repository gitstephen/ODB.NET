using System.Data;
using System.Linq.Expressions;

namespace UnitODB.Linq
{
	public interface IOdbVisitor
	{
		IDiagram Diagram { get; }

		bool HasCount { get; }

		void Clear();

		IDbDataParameter[] GetParamters();

		string Translate(Expression expression);
	}
}