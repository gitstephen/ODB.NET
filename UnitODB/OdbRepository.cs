using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;

namespace UnitODB
{
	public class OdbRepository<T> : IRepository<T>, IRepository, IEnumerable<T>, IEnumerable where T : IEntity
	{ 
		public IDbContext DbContext { get; private set; } 
		
		private IDiagram _diagram;
		public IDiagram Diagram
		{
			get
			{
				if (_diagram == null)
				{
					_diagram = new OdbDiagram(DbContext.Depth);
					_diagram.FetchTable(typeof(T));
				}

				return _diagram;
			}
		}
		
		private IQuery _entities;
		public IQuery Entities
		{
			get
			{
				if (_entities == null)
				{ 
					OdbTable t = this.Diagram.GetTable(typeof(T));

					this._entities = this.DbContext.Select<T>(this.Diagram.GetColumns()).As(t.Alias);

					foreach (var kv in this.Diagram.Tables)
					{
						this._entities.LeftJoin(kv.Key).On(kv.Value);
					}
				}

				return _entities;
			}
		} 

		public OdbRepository(IDbContext dbContext)
		{
			if (dbContext == null)
			{
				throw new ArgumentNullException("dbContext", "dbContext cannot be null.");
			}

			this.DbContext = dbContext;
		}

		public void SetDepth(int depth)
		{
			if (depth == 0)
			{
				throw new ArgumentNullException("depth", "Depth cannot be zero.");
			}

            DbContext.Depth = depth;

			if (_diagram != null)
			{
				_diagram.Clear();
				_diagram = null;
			}
			if (_entities != null)
			{
				_entities = null;
			}
		}

        public virtual IQuery Query(string[] cols)
        {
            return DbContext.Select<T>(cols);
        }

        public virtual T Get(int id)
		{
			var table = this.Diagram.GetTable(typeof(T));
 
			IQuery query = this.Entities.Where(table.Alias + "." + table.PK).Eq(id).Take(1);

			using (var enumerator = new EntityEnumerator<T>(query, Diagram))
			{
				var list = enumerator.ToList();

                return list[0];
			} 
		}	
		
		public virtual long Count()
		{
			IQuery q = DbContext.Count<T>();
			
			return DbContext.ExecuteScalar<long>(q); 
		}

		public virtual IList<T> ToList()
		{
			using (var enumerator = new EntityEnumerator<T>(Entities, Diagram))
			{
				return enumerator.ToList();
            } 
		} 
 
		public IEnumerator<T> GetEnumerator()
		{
			return new EntityEnumerator<T>(Entities, Diagram).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
