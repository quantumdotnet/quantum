namespace Quantum.QueryBuilder.MsSql.Transitions
{
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class InnerJoinTransition : IBuilderTransition
    {
        private readonly TableExpression _table;

        public InnerJoinTransition(TableExpression table)
        {
            _table = table;
        }

        public override string QueryPartValue
        {
            get { return "INNER JOIN " + _table.QueryPartValue; }
        }
    }
}