using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;

namespace UnitODB.Linq
{

	public static class OdbLinqExtension
	{
		public static IEntityQuery<T> EntityQueryable<T>(this IContainer container) where T : IEntity
		{ 
			return (container.Provider as IEntityProvider).CreateQuery<T>();
		}

		public static IList<T> Gets<T>(this IContainer container, Expression<Func<T, bool>> func) where T : IEntity
		{
			return container.EntityQueryable<T>().Where(func).ToList();
		}

		public static T First<T>(this IContainer container, Expression<Func<T, bool>> func) where T : IEntity
		{
			return container.EntityQueryable<T>().Where(func).FirstOrDefault();
		}

		public static int Count<T>(this IContainer container, Expression<Func<T, bool>> func) where T : IEntity
		{
			return container.EntityQueryable<T>().Where(func).Count();
		}
	}
}
