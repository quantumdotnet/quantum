namespace Quantum.Common
{
    public class ColumnSelectExpression : IQueryPart
    {
        private readonly string _value;

        public ColumnSelectExpression(string value)
        {
            _value = value;
        }

        public string QueryPartValue
        {
            get { return _value; }
        }

        public static implicit operator ColumnSelectExpression(string value)
        {
            return new ColumnSelectExpression(value);
        }

        public static implicit operator ColumnSelectExpression(SqlExpression expr)
        {
            return new ColumnSelectExpression(expr.QueryPartValue);
        }
    }
}