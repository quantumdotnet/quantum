namespace Quantum.Common
{
    public class SqlPredicate : IQueryPart
    {
        private readonly string _value;

        public SqlPredicate(string value)
        {
            _value = value;
        }

        public string QueryPartValue
        {
            get { return _value; }
        }

        public static implicit operator SqlPredicate(string value)
        {
            return new SqlPredicate(value);
        }

        public static SqlPredicate operator |(SqlPredicate left, SqlPredicate right)
        {
            return new SqlPredicate("(" + left.QueryPartValue + " OR " + right.QueryPartValue + ")");
        }

        public static SqlPredicate operator &(SqlPredicate left, SqlPredicate right)
        {
            return new SqlPredicate("(" + left.QueryPartValue + " AND " + right.QueryPartValue + ")");
        }

        public static SqlPredicate operator !(SqlPredicate predicate)
        {
            return new SqlPredicate("(NOT " + predicate.QueryPartValue + ")");
        }
    }
}