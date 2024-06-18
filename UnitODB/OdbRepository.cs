using System;
using System.Collections;
using System.Collections.Generic;

namespace UnitODB
{
	public class OdbRepository<T> : IRepository<T>, IRepository, IEnumerable<T>, IEnumerable where T : IEntity
	{ 
		public IContainer Unit { get; }
		
		private IDiagram _diagram;
		public IDiagram Diagram
		{
			get
			{
				if (_diagram == null)
				{
					_diagram = new OdbDiagram(Unit.Context.Depth);
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

					this._entities = this.Unit.Query<T>(this.Diagram.GetColumns()).As(t.Alias);

					foreach (var kv in this.Diagram.Tables)
					{
						this._entities.LeftJoin(kv.Key).On(kv.Value);
					}
				}

				return _entities;
			}
		} 

		public OdbRepository(IContainer unit)
		{
			if (unit == null)
			{
				throw new ArgumentNullException("Container");
			}

			Unit = unit;
		}

		public void SetDepth(int n)
		{
			if (n == 0)
			{
				throw new ArgumentNullException("Depth");
			}

			Unit.Context.Depth = n;

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

		public virtual T Get(int id)
		{
			var table = this.Diagram.GetTable(typeof(T));
 
			IQuery query = this.Entities.Where(table.Alias + "." + table.PK).Eq(id).Take(1);

			using (var et = new EntityEnumerator<T>(query, Diagram))
			{
				IList<T> list = OdbContext.Collection(et);

				if (list.Count == 0)
				{
					return default(T);
				}

				return list[0];
			} 
		}	
		
		public void Add(T t)
		{
			Unit.RegisterAdd(t);
		}

		public void Update(T t)
		{
			Unit.RegisterUpdate(t);
		}

		public virtual void Delete(T t)
		{
			Unit.RegisterDelete(t);
		}

		public virtual long Count()
		{
			IQuery q = Unit.Context.Count<T>();

			return Unit.Context.ExecuteScalar<long>(q); 
		}

		public virtual IList<T> ToList()
		{
			using (var et = new EntityEnumerator<T>(Entities, Diagram))
			{
				return OdbContext.Collection(et);
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
