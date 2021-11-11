namespace Rogero.FluentMigrator.Poco;

public static class NameHelper
{
    public static string GetForeignKeyName(
        string foreignTable,
        string foreignColumns,
        string primaryTable,
        string primaryColumns
    )
    {
        return $"FK__{foreignTable}__{foreignColumns}__TO__{primaryTable}__{primaryColumns}";
    }

    public static string GetForeignKeyName(
        string        foreignTable,
        IList<string> foreignColumns,
        string        primaryTable,
        IList<string> primaryColumns
    )
    {
        var fkColumns = foreignColumns.StringJoin(",");
        var pkColumns = primaryColumns.StringJoin(",");
            
        return GetForeignKeyName(foreignTable, fkColumns, primaryTable, pkColumns);
    }

    public static string ToSqlLength(this int length)
    {
        if (length == Int32.MaxValue) return "max";
        return length.ToString();
    }
}