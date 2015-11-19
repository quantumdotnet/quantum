namespace Quantum.QueryBuilder.MsSql.Transitions
{
    using System.Linq;
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class GroupByTransition : IBuilderTransition
    {
        private readonly SqlExpression[] _groupByColumns;

        public GroupByTransition(params SqlExpression[] columns)
        {
            _groupByColumns = columns;
        }

        public override string QueryPartValue
        {
            get { return "GROUP BY " + string.Join(", ", _groupByColumns.Select(c => c.QueryPartValue)); }
        }
    }
}