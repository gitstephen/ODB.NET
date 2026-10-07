using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnitODB;
using UnitODB.Linq;

namespace UnitODB.SQLite
{
    public class SQLiteVisitor : OdbVisitor
    { 
        public SQLiteVisitor(IOdbProvider provider, int d) : base(provider, d)
        {
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Select")
            {
                Visit(m.Arguments[0]);
                LambdaExpression lambdaExpression = (LambdaExpression)UnitODB.Linq.ExpressionVisitor.StripQuotes(m.Arguments[1]);
                if (lambdaExpression.Body.NodeType != ExpressionType.Parameter)
                {
                    string value = _sb.ToString();
                    _sb.Clear();
                    _sb.Append("SELECT ");
                    if (!this.HasCount)
                    {
                        Visit(lambdaExpression.Body);
                    }
                    _sb.Append(value);
                }
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                Visit(m.Arguments[0]);
                _sb.Append(" WHERE ");
                LambdaExpression lambdaExpression2 = (LambdaExpression)UnitODB.Linq.ExpressionVisitor.StripQuotes(m.Arguments[1]);
                Visit(lambdaExpression2.Body);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Skip")
            {
                Visit(m.Arguments[0]);
                SetLimit();
                _sb.Append(" OFFSET ");
                Visit(m.Arguments[1]);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Take")
            {
                Visit(m.Arguments[0]);
                SetLimit();
                Visit(m.Arguments[1]);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderBy")
            {
                Visit(m.Arguments[0]);
                _sb.Append(" ORDER BY ");
                LambdaExpression lambdaExpression3 = (LambdaExpression)UnitODB.Linq.ExpressionVisitor.StripQuotes(m.Arguments[1]);
                Visit(lambdaExpression3.Body);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderByDescending")
            {
                Visit(m.Arguments[0]);
                _sb.Append(" ORDER BY ");
                LambdaExpression lambdaExpression4 = (LambdaExpression)UnitODB.Linq.ExpressionVisitor.StripQuotes(m.Arguments[1]);
                Visit(lambdaExpression4.Body);
                _sb.Append(" DESC");
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "ThenBy")
            {
                Visit(m.Arguments[0]);
                _sb.Append(", ");
                LambdaExpression lambdaExpression5 = (LambdaExpression)UnitODB.Linq.ExpressionVisitor.StripQuotes(m.Arguments[1]);
                Visit(lambdaExpression5.Body);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "ThenByDescending")
            {
                Visit(m.Arguments[0]);
                _sb.Append(", ");
                LambdaExpression lambdaExpression6 = (LambdaExpression)UnitODB.Linq.ExpressionVisitor.StripQuotes(m.Arguments[1]);
                Visit(lambdaExpression6.Body);
                _sb.Append(" DESC");
            }
            else if (m.Method.Name == "Contains")
            {
                Visit(m.Object);
                _sb.Append(" LIKE ('%' || ");
                Visit(m.Arguments[0]);
                _sb.Append(" || '%')");
            }
            else if (m.Method.Name == "StartsWith")
            {
                Visit(m.Object);
                _sb.Append(" LIKE (");
                Visit(m.Arguments[0]);
                _sb.Append(" || '%')");
            }
            else if (m.Method.Name == "EndsWith")
            {
                Visit(m.Object);
                _sb.Append(" LIKE ('%' || ");
                Visit(m.Arguments[0]);
                _sb.Append(")");
            }
            else if (m.Method.Name == "Equals")
            {
                Visit(m.Object);
                _sb.Append(" = ");
                Visit(m.Arguments[0]);
            }
            else if (m.Method.Name == "Trim")
            {
                _sb.Append("TRIM(");
                Visit(m.Object);
                _sb.Append(")");
            }
            else if (m.Method.Name == "ToLower")
            {
                _sb.Append("LOWER(");
                Visit(m.Object);
                _sb.Append(")");
            }
            else if (m.Method.Name == "ToUpper")
            {
                _sb.Append("UPPER(");
                Visit(m.Object);
                _sb.Append(")");
            }
            else if (m.Method.Name == "IndexOf")
            {
                _sb.Append("INSTR(");
                Visit(m.Object);
                _sb.Append(", ");
                Visit(m.Arguments[0]);
                _sb.Append(")");
            }
            else if (m.Method.Name == "Substring")
            {
                _sb.Append("SUBSTR(");
                Visit(m.Object);
                _sb.Append(", ");
                Visit(m.Arguments[0]);
                _sb.Append(", ");
                Visit(m.Arguments[1]);
                _sb.Append(")");
            }
            else if (m.Method.Name == "FirstOrDefault")
            {
                Visit(m.Arguments[0]);
                SetLimit();
                _sb.Append(" 1");
            }
            else if (m.Method.Name == "Count")
            {
                SetCount();
                Visit(m.Arguments[0]);
            }
            else
            {
                if (!(m.Method.Name == "LongCount"))
                {
                    throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
                }
                SetCount();
                Visit(m.Arguments[0]);
            }
            return m;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(DateTime) && m.Member.Name == "Now")
            {
                _sb.Append("datetime()");
            }
            else if (m.Member.DeclaringType == typeof(string) && m.Member.Name == "Length")
            {
                _sb.Append("LENGTH(");
                Visit(m.Expression);
                _sb.Append(")");
            }
            else if (OdbType.OdbEntity.IsAssignableFrom(m.Type))
            {
                _sb.Append(GetColumns(m.Type));
            }
            else
            {
                VisitMemberValue(m);
            }
            return m;
        }

        public override string Translate(Expression expression)
        { 
            _expression = expression ?? throw new ArgumentNullException("expression");
           
            Clear();
            
            Visit(_expression);
            
            string sql = _sb.ToString();
            
            if (!sql.StartsWith("SELECT"))
            {
                if (!this.HasCount)
                {
                    Type elementType = TypeSystem.GetElementType(_expression.Type);
                    sql = GetColumns(elementType) + sql;
                }
                return "SELECT " + sql;
            }
            
            return sql;
        }
    }
}
