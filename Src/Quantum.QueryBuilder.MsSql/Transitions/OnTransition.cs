namespace Quantum.QueryBuilder.MsSql.Transitions
{
    using Quantum.QueryBuilder.Common;

    public class OnTransition : IBuilderTransition
    {
        private readonly string _condition;

        public OnTransition(string condition)
        {
            _condition = condition;
        }

        public override string QueryPartValue
        {
            get { return "ON " + _condition; }
        }
    }
}