namespace Rogero.FluentMigrator.Poco;

public record SchemaTableNames(
    string Schema,
    string Table)
{
    public override string ToString()
    {
        return $"{Schema}.{Table}";
    }

    public string ToStringBracketed()
    {
        return $"[{Schema}].[{Table}]";
    }
};