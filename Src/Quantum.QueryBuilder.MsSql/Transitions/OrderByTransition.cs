namespace Quantum.QueryBuilder.MsSql.Transitions
{
    using System.Linq;
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class OrderByTransition : IBuilderTransition
    {
        private readonly SqlExpression[] _columns;

        public OrderByTransition(params SqlExpression[] columns)
        {
            _columns = columns;
        }

        public override string QueryPartValue
        {
            get { return "ORDER BY " + string.Join(", ", _columns.Select(c => c.QueryPartValue)); }
        }
    }
}