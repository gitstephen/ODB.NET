using System;
using System.Collections.Generic;

namespace UnitODB
{
	public class OdbDiagram : IDiagram
	{
		private List<string> _cols;

		private Dictionary<string, string> _tables;

		private int level;

		private Dictionary<string, OdbTable> _nodes { get; set; }

		public int Depth { get; set; }

		public OdbTable Root { get; private set; }

		public Dictionary<string, string> Tables
		{
			get
			{
				if (_tables.Count == 0)
				{
					getNodes(Root);
				}
				return _tables;
			}
		}

		public OdbDiagram(int depth)
		{
			Depth = depth;

			_nodes = new Dictionary<string, OdbTable>();
			_tables = new Dictionary<string, string>();
			_cols = new List<string>();
		}

		public void FetchTable(Type type)
		{
			OdbTable odbTable = OdbMapping.CreateTable(type);

			if (Root == null || Root != odbTable)
			{
				Clear();

				Root = odbTable;
				_nodes.Add(Root.Name, Root);

				level = 0;

				findNodes(Root);
			}
		}

		private void findNodes(OdbTable node)
		{
			foreach (OdbColumn column in node.Columns)
			{
				if (column.Attribute.IsForeignKey && level < Depth - 1)
				{
					level++;

					OdbTable odbTable = OdbMapping.CreateTable(column.GetMapType());

					odbTable.Id = _nodes.Count;
					odbTable.Parent = node.Id;
					odbTable.FK = column.Name;

					_nodes.Add(odbTable.Name, odbTable);

					findNodes(odbTable);

					level--;
				}
			}
		}

		public OdbTable GetTable(string name)
		{
			if (_nodes.ContainsKey(name))
			{
				return _nodes[name];
			}

			return null;
		}

		public OdbTable GetTable(Type type)
		{
			string tableName = OdbMapping.GetTableName(type);
			return GetTable(tableName);
		}

		private void getNodes(OdbTable root)
		{
			foreach (OdbTable value in _nodes.Values)
			{
				if (value.Parent == root.Id)
				{
					string key = Enclosed(value.Name) + " AS " + value.Alias;
					string text = Enclosed(value.Alias) + "." + Enclosed(value.PK);
					string text2 = Enclosed(root.Alias) + "." + Enclosed(value.FK);

					_tables.Add(key, text + " = " + text2);

					getNodes(value);
				}
			}
		}

		public string[] GetColumns()
		{
			return GetColumns(Root);
		}

		public string[] GetColumns(Type type)
		{
			OdbTable table = GetTable(type);
			return GetColumns(table);
		}

		public string[] GetColumns(OdbTable table)
		{
			if (_cols.Count == 0)
			{
				getColumns(table);
			}
			return _cols.ToArray();
		}

		private void getColumns(OdbTable node)
		{
			foreach (OdbColumn column in node.Columns)
			{
				string text = Enclosed(node.Alias) + "." + Enclosed(column.Name);
				string text2 = Enclosed(node.Alias + "." + column.Name);
				_cols.Add(text + " AS " + text2);
			}

			foreach (OdbTable value in _nodes.Values)
			{
				if (value.Parent == node.Id)
				{
					getColumns(value);
				}
			}
		}

		public static string Enclosed(string str)
		{
			return "[" + str + "]";
		}

		public void Clear()
		{
			if (_nodes != null)
			{
				_nodes.Clear();
			}

			if (_cols != null)
			{
				_cols.Clear();
			}

			if (_tables != null)
			{
				_tables.Clear();
			}
		}
	}
}
