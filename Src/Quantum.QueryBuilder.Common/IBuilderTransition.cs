namespace Quantum.QueryBuilder.Common
{
    using Quantum.Common;

    public abstract class IBuilderTransition : IQueryPart
    {
        public abstract string QueryPartValue { get; }
    }
}