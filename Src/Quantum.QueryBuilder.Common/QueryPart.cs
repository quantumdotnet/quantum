namespace Quantum.QueryBuilder.Common
{
    using System;

    public abstract class QueryPart
    {
        public abstract string BuildQuery();
    }

    public class NilQueryPart : QueryPart
    {
        public override string BuildQuery()
        {
            return string.Empty;
        }
    }

    public abstract class ConsQueryPart : QueryPart
    {
        private readonly QueryPart _tail;

        protected ConsQueryPart(QueryPart tail)
        {
            _tail = tail;
        }

        public override string BuildQuery()
        {
            return _tail.BuildQuery() + Environment.NewLine + BuildPartQuery();
        }

        protected abstract string BuildPartQuery();
    }
}