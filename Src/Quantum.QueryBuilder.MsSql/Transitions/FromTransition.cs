namespace Quantum.QueryBuilder.MsSql.Transitions
{
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class FromTransition : IBuilderTransition
    {
        private readonly TableExpression _table;

        public FromTransition(TableExpression table)
        {
            _table = table;
        }

        public override string QueryPartValue
        {
            get { return "FROM " + _table.QueryPartValue; }
        }
    }
}