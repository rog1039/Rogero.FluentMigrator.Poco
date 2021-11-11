namespace Rogero.FluentMigrator.Poco;

public record ColumnDataName(string Name)
{
    public override string ToString()
    {
        return Name;
    }
}