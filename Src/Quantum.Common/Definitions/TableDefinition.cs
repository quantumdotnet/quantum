using System;

namespace Quantum.Common.Definitions
{
    public class TableDefinition : IAliasProvider
    {
        private readonly string _value;

        public TableDefinition(string value)
        {
            _value = value;
        }

        public string Parent
        {
            get { return Value; }
        }

        public string Value
        {
            get { return _value; }
        }

        public static implicit operator TableExpression (TableDefinition def)
        {
            return new TableExpression(def.Value);
        }
    }
}