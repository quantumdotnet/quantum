namespace Quantum.Common
{
    public class TableExpression : IQueryPart
    {
        private readonly string _value;

        public TableExpression(string value)
        {
            _value = value;
        }

        public string QueryPartValue
        {
            get { return _value; }
        }

        public static implicit operator TableExpression(string value)
        {
            return new TableExpression(value);
        }
    }
}