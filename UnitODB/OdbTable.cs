using System;
using System.Collections.Generic;

namespace UnitODB
{
	public class OdbTable
	{
		public int Id { get; set; }

		public string Name { get; private set; }

		public int Parent { get; set; }

		public string FK { get; set; }

		public string PK { get; set; }

		public List<OdbColumn> Columns { get; private set; }

		public string Alias => "T" + Id;

		public OdbTable(Type type)
		{
			Id = 0;
			Parent = -1;
			Name = OdbMapping.GetTableName(type);
			Columns = new List<OdbColumn>();
		}
	}
}
