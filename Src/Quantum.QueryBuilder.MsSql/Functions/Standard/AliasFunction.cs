namespace Quantum.QueryBuilder.MsSql.Functions.Standard
{
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class AliasFunction : ISqlFunction<Alias>
    {
        private readonly string _alias;

        public AliasFunction(string alias)
        {
            _alias = alias;
        }

        public Alias Execute()
        {
            return new Alias(_alias);
        }
    }
}