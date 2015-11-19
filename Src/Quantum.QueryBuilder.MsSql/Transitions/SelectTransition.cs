namespace Quantum.QueryBuilder.MsSql.Transitions
{
    using System.Linq;
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class SelectTransition : IBuilderTransition
    {
        private readonly ColumnSelectExpression[] _columns;

        public SelectTransition(params ColumnSelectExpression[] columns)
        {
            _columns = columns;
        }

        public override string QueryPartValue
        {
            get { return "SELECT " + string.Join(", ", _columns.Select(column => column.QueryPartValue)); }
        }
    }
}