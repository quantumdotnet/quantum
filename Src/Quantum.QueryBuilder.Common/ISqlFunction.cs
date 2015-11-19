namespace Quantum.QueryBuilder.Common
{
    public interface ISqlFunction<out TResult>
    {
        TResult Execute();
    }
}