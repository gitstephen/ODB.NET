using System;
using UnitODB;

namespace UnitODB.SQLite
{
    public class SQLiteQuery<T> : OdbQuery<T>
    {
		private bool _limit;

		public SQLiteQuery(IDbContext context) : base(context)
		{
			_limit = false;
		}

		public override IQuery Skip(int start)
		{
			SetLimit();
			_sb.Append(start.ToString());
			return this;
		}

		public override IQuery Take(int n)
		{
			if (!_limit)
			{
				Skip(0);
			}
			_sb.Append(" , " + n);
			return this;
		}

		private void SetLimit()
		{
			if (!_limit)
			{
				_sb.Append(" LIMIT ");
				_limit = true;
			}
		}

		public override string ToString()
		{
			return _sb.ToString();
		}
	}
}
