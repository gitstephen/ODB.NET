using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace UnitODB.Linq
{
	public abstract class OdbEntityProvider : OdbProvider, IEntityProvider, IQueryProvider, IProvider
	{
		public IOdbVisitor Visitor { get; set; }
		public IDbContext DbContext { get; set; }   

		protected OdbEntityProvider()
		{
			this.DbContext = this.CreateContext();
		}

		public IQueryable CreateQuery(Expression expression)
		{
			Type elementType = TypeSystem.GetElementType(expression.Type);
			try
			{
				return (IQueryable)Activator.CreateInstance(typeof(EntityQuery<>).MakeGenericType(elementType), this, expression);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

		public IQueryable<T> CreateQuery<T>(Expression expression)
		{
			return new EntityQuery<T>(this, expression);
		} 

		public virtual object Execute(Expression expression)
		{
			Type elementType = TypeSystem.GetElementType(expression.Type);

			if (elementType.Name.Contains("Anonymous"))
			{
				throw new NotSupportedException("Anonymous Expression");
			}

			string sql = Translate(expression);

			IDataReader dataReader = DbContext.ExecuteReader(sql, Visitor.GetParamters());

			if (OdbType.OdbEntity.IsAssignableFrom(elementType))
			{
				return Activator.CreateInstance(typeof(EntityEnumerator<>).MakeGenericType(elementType), dataReader, Visitor.Diagram);
			}

			return Activator.CreateInstance(typeof(GenericEnumerator<>).MakeGenericType(elementType), dataReader);
		}

		public virtual T Execute<T>(Expression expression)
		{
			bool num = typeof(T).Name == "IEnumerable`1";

			object obj = Execute(expression);

			if (num)
			{
				return (T)obj;
			}

			return (obj as IEnumerable).OfType<T>().FirstOrDefault();
		}

		public abstract string Translate(Expression expression);
	}
}