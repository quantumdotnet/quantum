namespace Quantum.QueryBuilder.MsSql.Functions.Standard
{
    using Quantum.Common;
    using Quantum.QueryBuilder.Common;

    public class ValueFunction : ISqlFunction<SqlExpression>
    {
        private readonly string _expression;

        public ValueFunction(string expression)
        {
            _expression = expression;
        }

        public SqlExpression Execute()
        {
            return new SqlExpression(_expression);
        }
    }
}