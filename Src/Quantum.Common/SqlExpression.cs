namespace Quantum.Common
{
    public class SqlExpression : IQueryPart
    {
        private readonly string _value;

        public SqlExpression(string value)
        {
            _value = value;
        }

        public string QueryPartValue
        {
            get { return _value; }
        }

        public ColumnSelectExpression As(string alias)
        {
            return new ColumnSelectExpression(QueryPartValue + " AS " + alias);
        }

        public static implicit operator SqlExpression(string value)
        {
            return new SqlExpression("'" + value + "'");
        }

        public ColumnSelectExpression this[string alias]
        {
            get { return As(alias); }
        }

        public static SqlPredicate operator <(SqlExpression left, SqlExpression right)
        {
            return new SqlPredicate("(" + left.QueryPartValue + " < " + right.QueryPartValue + ")");
        }

        public static SqlPredicate operator >(SqlExpression left, SqlExpression right)
        {
            return new SqlPredicate("(" + left.QueryPartValue + " > " + right.QueryPartValue + ")");
        }

        public static SqlPredicate operator <=(SqlExpression left, SqlExpression right)
        {
            return new SqlPredicate("(" + left.QueryPartValue + " <= " + right.QueryPartValue + ")");
        }

        public static SqlPredicate operator >=(SqlExpression left, SqlExpression right)
        {
            return new SqlPredicate("(" + left.QueryPartValue + " >= " + right.QueryPartValue + ")");
        }

        public static ColumnSelectExpression operator <(SqlExpression expr, Alias alias)
        {
            return expr.As(alias.Value);
        }

        public static ColumnSelectExpression operator > (SqlExpression expr, Alias alias)
        {
            return expr.As(alias.Value);
        }

        public static SqlValueExpression operator <(SqlExpression expr, TildeSqlExpression alias)
        {
            return null;
        }

        public static SqlValueExpression operator > (SqlExpression expr, TildeSqlExpression alias)
        {
            return null;
        }

        public static SqlExpression operator +(SqlExpression left, SqlExpression right)
        {
            return new SqlExpression("(" + left.QueryPartValue + " + " + right.QueryPartValue + ") ");
        }

        public static TildeSqlExpression operator ~(SqlExpression expr)
        {
            return new TildeSqlExpression(expr);
        }
    }

    public class TildeSqlExpression
    {
        private readonly SqlExpression _expr;

        public TildeSqlExpression(SqlExpression expr)
        {
            _expr = expr;
        }
    }

    public class Alias
    {
        private readonly string _alias;

        public Alias(string alias)
        {
            _alias = alias;
        }

        public string Value
        {
            get { return _alias; }
        }


        public static Alias operator ~(Alias alias)
        {
            return alias;
        }
    }

    public class AliasHelper
    {
        private readonly SqlExpression _expr;

        public AliasHelper(SqlExpression expr)
        {
            _expr = expr;
        }

        public static ColumnSelectExpression operator >(AliasHelper helper, string alias)
        {
            return helper._expr.As(alias);
        }

        public static ColumnSelectExpression operator <(AliasHelper helper, string alias)
        {
            return helper._expr.As(alias);
        }
    }
}