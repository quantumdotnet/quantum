namespace Quantum.QueryBuilder.MsSql.Transitions
{
    using System.Linq;
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class WhereTransition : IBuilderTransition
    {
        private readonly SqlPredicate[] _predicates;

        public WhereTransition(params SqlPredicate[] predicates)
        {
            _predicates = predicates;
        }

        public override string QueryPartValue
        {
            get
            {
                SqlPredicate finalPredicate = _predicates
                    .Skip(1)
                    .Aggregate(
                        _predicates.First(), 
                        (predicate, result) => predicate & result);

                return "WHERE " + finalPredicate.QueryPartValue;
            }
        }
    }
}