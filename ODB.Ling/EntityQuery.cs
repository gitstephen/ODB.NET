using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace UnitODB.Linq
{
	public class EntityQuery<T> : IEntityQuery<T>, IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T>, IEnumerable, IQueryable, IOrderedQueryable
	{
		public IQueryProvider Provider { get; private set; }

		public Expression Expression { get; private set; }

		public Type ElementType => typeof(T);

		public EntityQuery(IEntityProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			Provider = provider;

			Expression = Expression.Constant(this);
		}

		public EntityQuery(IQueryProvider provider, Expression expression)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}
			if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
			{
				throw new ArgumentOutOfRangeException("expression");
			}

			Provider = provider;

			Expression = expression;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual string GetSQL()
		{
			return (Provider as IEntityProvider).Translate(Expression);
		}
	}
}
