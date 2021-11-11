namespace Rogero.FluentMigrator.Poco.Attributes;

public class ColumnNameAttribute : Attribute
{
    public string ColumnName { get; }

    public ColumnNameAttribute(string columnName)
    {
        ColumnName = columnName;
    }
}