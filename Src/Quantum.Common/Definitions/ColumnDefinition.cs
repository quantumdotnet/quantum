namespace Quantum.Common.Definitions
{
    public class ColumnDefinition : SqlExpression
    {
        private readonly string _value;

        public ColumnDefinition(string value) : base(value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }
    }
}