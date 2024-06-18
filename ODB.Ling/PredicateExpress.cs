using System;
using System.Linq;
using System.Linq.Expressions;

namespace UnitODB.Linq
{
	public static class PredicateExpress
	{
		public static Expression<Func<T, bool>> True<T>() where T : IEntity
		{
			return (T f) => true;
		}

		public static Expression<Func<T, bool>> False<T>() where T : IEntity
		{
			return (T f) => false;
		}

		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
		{
			InvocationExpression right = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, right), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
		{
			InvocationExpression right = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, right), expr1.Parameters);
		}
	}

}