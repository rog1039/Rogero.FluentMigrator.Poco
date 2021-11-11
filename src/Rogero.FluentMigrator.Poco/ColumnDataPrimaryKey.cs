namespace Rogero.FluentMigrator.Poco;

public record ColumnDataPrimaryKey(bool IsPrimaryKey = false)
{
    public override string ToString()
    {
        return IsPrimaryKey ? "PrimaryKey" : "---";
    }
}