using System;
using System.Data;

namespace UnitODB
{ 
	public class EntityEnumerator<T> : GenericEnumerator<T> where T : IEntity
	{
		private int level;

		public IDiagram Diagram { get; private set; }

		public EntityEnumerator(IQuery q, IDiagram diagram)
			: base(q)
		{
			Diagram = diagram;
			level = 0;
		}

		public override object GetEntity(Type type)
		{
			object obj = Activator.CreateInstance(type);
			OdbTable table = Diagram.GetTable(type);
			
			if (table == null)
			{
				throw new OdbException("No table.");
			}

			foreach (OdbColumn column in table.Columns)
			{
				object value = GetValue(table.Alias + "." + column.Name);

				if (!column.Attribute.IsForeignKey)
				{
					column.SetValue(obj, value);
				}
				else if (value != null && level < Diagram.Depth - 1)
				{
					level++;
				
					Type mapType = column.GetMapType();
					
					IEntity entity = GetEntity(mapType) as IEntity;

					entity.ModelState = true;

					level--;
					
					if (entity.Id != 0)
					{
						column.SetValue(obj, entity);
					}
				}
			}

			
			return obj;
		}
	}
}
