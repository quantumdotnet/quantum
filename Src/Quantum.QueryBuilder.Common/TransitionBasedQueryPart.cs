namespace Quantum.QueryBuilder.Common
{
    public class TransitionBasedQueryPart : ConsQueryPart
    {
        private readonly IBuilderTransition _transition;

        public TransitionBasedQueryPart(QueryPart tail, IBuilderTransition transition) : base(tail)
        {
            _transition = transition;
        }

        protected override string BuildPartQuery()
        {
            return _transition.QueryPartValue;
        }
    }
}