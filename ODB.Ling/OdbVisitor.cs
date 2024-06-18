using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace UnitODB.Linq
{
	public abstract class OdbVisitor : ExpressionVisitor, IOdbVisitor
	{
		protected StringBuilder _sb;

		protected Expression _expression;		

		protected int _index;

		protected string _limit = ""; 

		protected bool _is_count;

		public List<IDbDataParameter> Parameters;

		public IOdbProvider Provider { get; set; } 
		public IDiagram Diagram { get; } 
		public bool HasCount => _is_count;

		public OdbVisitor(IOdbProvider provider, int depth)
		{
			Provider = provider ?? throw new ArgumentNullException("provider");

			Diagram = new OdbDiagram(depth);

			Parameters = new List<IDbDataParameter>();
			
			_sb = new StringBuilder();
		}

		protected override Expression VisitBinary(BinaryExpression b)
		{
			Visit(b.Left);

			switch (b.NodeType)
			{
				case ExpressionType.AndAlso:
					_sb.Append(" AND ");
					break;
				case ExpressionType.OrElse:
					_sb.Append(" OR ");
					break;
				case ExpressionType.Equal:
					if (IsNullConstant(b.Right))
					{
						_sb.Append(" IS ");
					}
					else
					{
						_sb.Append(" = ");
					}
					break;
				case ExpressionType.NotEqual:
					if (IsNullConstant(b.Right))
					{
						_sb.Append(" IS NOT ");
					}
					else
					{
						_sb.Append(" <> ");
					}
					break;
				case ExpressionType.LessThan:
					_sb.Append(" < ");
					break;
				case ExpressionType.LessThanOrEqual:
					_sb.Append(" <= ");
					break;
				case ExpressionType.GreaterThan:
					_sb.Append(" > ");
					break;
				case ExpressionType.GreaterThanOrEqual:
					_sb.Append(" >= ");
					break;
				default:
					throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
			}

			Visit(b.Right);
			return b;
		}

		protected override Expression VisitUnary(UnaryExpression u)
		{
			switch (u.NodeType)
			{
				case ExpressionType.Not:
					_sb.Append(" NOT ");
					Visit(u.Operand);
					break;
				case ExpressionType.Convert:
					Visit(u.Operand);
					break;
				default:
					throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
			}

			return u;
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			if (c.Value is IQueryable queryable)
			{
				Diagram.FetchTable(queryable.ElementType);
				OdbTable table = Diagram.GetTable(queryable.ElementType);
				_sb.Append(" FROM ");
				_sb.Append(Enclosed(table.Name));
				_sb.Append(" AS ");
				_sb.Append(table.Alias);
				{
					foreach (KeyValuePair<string, string> table2 in Diagram.Tables)
					{
						_sb.Append(" LEFT JOIN ");
						_sb.Append(table2.Key);
						_sb.Append(" ON ");
						_sb.Append(table2.Value);
					}
					return c;
				}
			}

			AddParamter(c);
			return c;
		}

		protected override NewExpression VisitNew(NewExpression nex)
		{
			List<string> list = new List<string>();
			int num = 0;
			foreach (string item in VisitMemberList(nex.Arguments))
			{
				list.Add(item + " AS " + nex.Members[num++].Name);
			}
			if (list.Count > 0)
			{
				_sb.Append(string.Join(",", list.ToArray()));
			}

			return nex;
		}

		protected override Expression VisitMemberInit(MemberInitExpression init)
		{
			VisitNew(init.NewExpression);
			VisitBindingList(init.Bindings);
			return init;
		}

		protected virtual IEnumerable<string> VisitMemberList(ReadOnlyCollection<Expression> original)
		{
			int i = 0;
			while (i < original.Count)
			{
				yield return GetColumnName(original[i++] as MemberExpression);
			}
		}

		protected override IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
		{
			int num = 0;
			while (num < original.Count)
			{
				MemberBinding memberBinding = VisitBinding(original[num++]);
				_sb.Append(" AS " + memberBinding.Member.Name);
				if (num < original.Count)
				{
					_sb.Append(",");
				}
			}
			return original;
		}

		public virtual string GetColumnName(MemberExpression m)
		{
			if (OdbType.OdbEntity.IsAssignableFrom(m.Expression.Type))
			{
				OdbTable table = Diagram.GetTable(m.Expression.Type);
				if (table != null)
				{
					return Enclosed(table.Alias) + "." + Enclosed(m.Member.Name);
				}
			}
			return m.Member.Name;
		}

		public Expression VisitMemberValue(MemberExpression m)
		{
			if (m.Member is FieldInfo)
			{
				FieldInfo fieldInfo = m.Member as FieldInfo;
				ConstantExpression constantExpression = m.Expression as ConstantExpression;
				if (fieldInfo != null && constantExpression != null)
				{
					object value = fieldInfo.GetValue(constantExpression.Value);
					Visit(Expression.Constant(value));
				}
			}
			else if (m.Member is PropertyInfo)
			{
				MemberExpression memberExpression = m;
				string text = m.Member.Name;
				while (memberExpression.Expression is MemberExpression)
				{
					memberExpression = memberExpression.Expression as MemberExpression;
					text = memberExpression.Member.Name + "." + text;
				}
				if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
				{
					_sb.Append(GetColumnName(m));
				}
				else if (memberExpression.Expression.NodeType == ExpressionType.Constant)
				{
					ConstantExpression constantExpression2 = (ConstantExpression)memberExpression.Expression;
					object value2 = GetValue(((FieldInfo)memberExpression.Member).GetValue(constantExpression2.Value), text);
					Visit(Expression.Constant(value2));
				}
			}

			return m;
		}

		public static object GetValue(object obj, string propertyName)
		{
			string[] array = propertyName.Split('.');
			int num = 1;
			while (obj != null && num < array.Length)
			{
				PropertyInfo property = obj.GetType().GetProperty(array[num++]);
				obj = property == null ? null : property.GetValue(obj);
			}
			return obj;
		}

		public virtual string GetColumns(Type type)
		{
			return string.Join(",", Diagram.GetColumns(type));
		}

		public virtual void AddParamter(ConstantExpression c)
		{
			if (c.Value == null)
			{
				_sb.Append("NULL");
				return;
			}

			IDbDataParameter dbDataParameter = Provider.CreateParameter(_index++, c.Value);

			Parameters.Add(dbDataParameter);

			_sb.Append(dbDataParameter.ParameterName);
		}

		public IDbDataParameter[] GetParamters()
		{
			return Parameters.ToArray();
		}

		protected static string Enclosed(string str)
		{
			return "[" + str + "]";
		}

		public virtual void SetLimit()
		{
			if (_limit == "")
			{
				_limit = " LIMIT ";
				_sb.Append(_limit);
			}
		}

		public virtual void SetCount()
		{
			if (!_is_count)
			{
				_is_count = true;
				_sb.Append("COUNT(*)");
			}
		}

		public void Clear()
		{
			Parameters.Clear();
			_sb.Clear();
			_index = 0;
			_limit = "";
			_is_count = false;
		}

		public abstract string Translate(Expression expression);
	}
}