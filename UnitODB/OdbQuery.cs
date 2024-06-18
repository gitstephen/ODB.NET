using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace UnitODB
{	
	public abstract class OdbQuery<T> : IQuery<T>, IQuery
	{
		protected StringBuilder _sb;
		
		private List<IDbDataParameter> parameters;

		private string _alias;
		private string _table;
				 
		public IDbContext Context { get; private set; }

		public string Table
		{
			get
			{
				if (string.IsNullOrEmpty(_table))
				{
					_table = OdbMapping.GetTableName(typeof(T));
				}
				return _table;
			}
			set
			{
				_table = value;
			}
		}

		public OdbQuery(IDbContext context)
        {
			Context = context;

            _sb = new StringBuilder();
            parameters = new List<IDbDataParameter>(); 
        }

        public virtual IQuery Select(string[] cols)
		{
			_sb.Append("SELECT ");
			_sb.Append(string.Join(",", cols));
			return this;
		}

		public virtual IQuery From()
		{
			return From(Table);
		}

		public virtual IQuery From(string table)
		{
			_sb.Append(" FROM ");
			_sb.Append(Enclosed(table));
			return this;
		}

		public virtual IQuery As(string str)
		{
			_sb.Append(" AS ");
			_sb.Append(Enclosed(str));
			return this;
		}

		public virtual IQuery Insert(string[] cols)
		{
			_sb.Append("INSERT INTO ");
			_sb.Append(Enclosed(Table));
			_sb.Append(" (");
			_sb.Append(string.Join(", ", cols));
			_sb.Append(")");
			return this;
		}

		public virtual IQuery Values(string[] cols)
		{
			_sb.Append(" VALUES (");
			_sb.Append(string.Join(", ", cols));
			_sb.Append(")");
			return this;
		}

		public virtual IQuery Update()
		{
			return Update(Table);
		}

		public virtual IQuery Update(string table)
		{
			_sb.Append("UPDATE ");
			_sb.Append(Enclosed(table));
			return this;
		}

		public virtual IQuery Set(string[] cols)
		{
			_sb.Append(" SET ");
			_sb.Append(string.Join(", ", cols));
			return this;
		}

		public virtual IQuery Delete()
		{
			_sb.Append("DELETE");
			return From();
		}

		public virtual IQuery Where(string str)
		{
			_sb.Append(" WHERE ");
			_sb.Append(AddAlias(str));
			return this;
		}

		public virtual IQuery And(string str)
		{
			_sb.Append(" AND ");
			_sb.Append(AddAlias(str));
			return this;
		}

		public virtual IQuery Or(string str)
		{
			_sb.Append(" OR ");
			_sb.Append(AddAlias(str));
			return this;
		}

		public virtual IQuery Equal(string str)
		{
			_sb.Append(" = ");
			_sb.Append(str);
			return this;
		}

		public virtual IQuery Eq(object val)
		{
			_sb.Append(" = ");
			return AddParams(val);
		}

		public virtual IQuery NotEq(object val)
		{
			_sb.Append(" <> ");
			return AddParams(val);
		}

		public virtual IQuery Gt(object val)
		{
			_sb.Append(" > ");
			return AddParams(val);
		}

		public virtual IQuery Lt(object val)
		{
			_sb.Append(" < ");
			return AddParams(val);
		}

		public virtual IQuery Gte(object val)
		{
			_sb.Append(" >= ");
			return AddParams(val);
		}

		public virtual IQuery Lte(object val)
		{
			_sb.Append(" <= ");
			return AddParams(val);
		}

		public IQuery Is(string str)
		{
			_sb.Append(" IS " + str);
			return this;
		}

		public virtual IQuery Like(string str)
		{
			_sb.Append(" LIKE ");
			return AddParams("%" + str + "%");
		}

		public virtual IQuery OrderBy(string str)
		{
			_sb.Append(" ORDER BY ");
			_sb.Append(AddAlias(str));
			return this;
		}

		public IQuery Group(string str)
		{
			_sb.Append(" GROUP BY " + str);
			return this;
		}

		public IQuery Having(string str)
		{
			_sb.Append(" HAVING ");
			return this;
		}

		public virtual IQuery SortAsc()
		{
			_sb.Append(" ASC");
			return this;
		}

		public virtual IQuery SortDesc()
		{
			_sb.Append(" DESC");
			return this;
		}

		public virtual IQuery Join<T1>() where T1 : IEntity
		{
			Type typeFromHandle = typeof(T1);
			return Join(OdbMapping.GetTableName(typeFromHandle));
		}

		public virtual IQuery Join(string table)
		{
			_sb.Append(" JOIN ");
			_sb.Append(Enclosed(table));
			return this;
		}

		public virtual IQuery LeftJoin<T1>() where T1 : IEntity
		{
			Type typeFromHandle = typeof(T1);
			return LeftJoin(OdbMapping.GetTableName(typeFromHandle));
		}

		public virtual IQuery LeftJoin(string table)
		{
			_sb.Append(" LEFT");
			return Join(table);
		}

		public virtual IQuery On(string str)
		{
			_sb.Append(" ON ");
			_sb.Append(str);
			return this;
		}

		public IQuery Not()
		{
			_sb.Append(" NOT ");
			return this;
		}

		public virtual IQuery Count(string str)
		{
			string[] cols = new string[1] { "COUNT(" + str + ")" };
			return Select(cols);
		}

		public abstract IQuery Skip(int start);

		public abstract IQuery Take(int count);

		public virtual string AddAlias(string str)
		{
			if (!string.IsNullOrEmpty(_alias))
			{
				return Enclosed(_alias) + "." + Enclosed(str);
			}
			return Enclosed(str);
		}

		public virtual IQuery Append(string str)
		{
			_sb.Append(str);
			return this;
		}

		protected static string Enclosed(string str)
		{
			if (str.IndexOf('[') == -1)
			{
				return "[" + str + "]";
			}
			return str;
		} 

		public virtual string SetParams(object b)
		{
			if (b == null)
			{
				return "NULL";
			}

			int count = parameters.Count;

			IDbDataParameter dbParameter = Context.CreateParameter(count, b);
			parameters.Add(dbParameter);
			
			return dbParameter.ParameterName;
		}

		public virtual IQuery<T> AddParams(object b)
		{
			string value = SetParams(b);
			_sb.Append(value);
			
			return this;
		}

		public IDbDataParameter[] GetParams()
		{
			return parameters.ToArray();
		}	

		public IDataReader Read()
		{
			return Context.ExecuteReader(this);
		}

		public virtual int Execute()
		{
			return Context.ExecuteNonQuery(this);
		}

		public virtual int ExecuteReturnId()
		{
			return Context.ExecuteReturnId(this);
		}

		public virtual T1 Single<T1>()
		{
			return Context.ExecuteSingle<T1>(this);
		} 

		public virtual IList<T> ToList()
		{
			return ToList<T>();
		}

		public virtual IList<T1> ToList<T1>()
		{
			return Context.ExecuteList<T1>(this);
		}
	}
}
