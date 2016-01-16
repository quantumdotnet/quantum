namespace Quantum.Common.Definitions
{
    public class ColumnDefinition : SqlExpression
    {
        public ColumnDefinition(string columnName, IAliasProvider alias = null) : 
            base(alias == null ? columnName : string.Format("{0}.{1}", alias.Parent, columnName))
        {
        }
    }
}