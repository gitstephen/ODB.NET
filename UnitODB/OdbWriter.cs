using System;
using System.Collections.Generic;

namespace UnitODB
{
	public class OdbWriter
	{
		private int level;

		private IDbContext _db;

		public OdbWriter(IDbContext db)
		{
			_db = db;
			level = 0;
		}

		public int Write<T>(T t) where T : IEntity
		{
			Type type = t.GetType();
			IQuery query = _db.BuildQuery<T>();
			query.Table = OdbMapping.GetTableName(type);

			List<string> cols = new List<string>();
			List<string> values = new List<string>();
			
			foreach (OdbColumn column in OdbMapping.GetColumns(type))
			{
				if (column.Attribute.IsAuto)
				{
					continue;
				}

				object obj = column.GetValue(t);
				
				if (column.Attribute.IsForeignKey)
				{
					if (level < _db.Depth - 1)
					{
						if (obj != null)
						{
							IEntity entity = obj as IEntity;
							level++;

							if (entity.Id == 0)
							{
								Write(entity);
							} 
							else
							{
								obj = entity.Id;
							}							
						}

						string item = query.SetParams(obj);
						
						values.Add(item);
						cols.Add("[" + column.Name + "]");

						level--;
					}
				}
				else if (!column.Attribute.IsList)
				{
					string item2 = query.SetParams(obj);
					
					values.Add(item2);
					cols.Add("[" + column.Name + "]");
				}
			}

			if (t.Id != 0)
			{
				List<string> cv = new List<string>();

				for (int i = 0; i < cols.Count; i++)
				{
                    cv.Add(cols[i] + " = " + values[i]);
				}

				query.Update().Set(cv.ToArray()).Where("Id").Eq(t.Id);
				query.Execute();

				return t.Id;
			}

			query.Insert(cols.ToArray()).Values(values.ToArray());

			return query.ExecuteReturnId();
		}
	}
}
