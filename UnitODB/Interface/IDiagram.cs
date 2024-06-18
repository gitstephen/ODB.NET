using System;
using System.Collections.Generic;

namespace UnitODB
{
	public interface IDiagram
	{
		int Depth { get; set; }

		Dictionary<string, string> Tables { get; }

		void FetchTable(Type type);

		OdbTable GetTable(Type type);

		string[] GetColumns();

		string[] GetColumns(Type type);

		void Clear();
	}
}
