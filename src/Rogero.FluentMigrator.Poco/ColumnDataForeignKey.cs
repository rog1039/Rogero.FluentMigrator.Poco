using System.Data;

namespace Rogero.FluentMigrator.Poco;

public record ColumnDataForeignKey(
    string ForeignSchemaName,
    string ForeignTableName,
    string ForeignColumnNames,
    string PrimarySchemaName,
    string PrimaryTableName,
    string PrimaryColumnNames,
    Rule   CascadeDeleteRule)
{
    public string? GroupId    { get; set; } = String.Empty;
    public bool    IsMultiKey => !string.IsNullOrWhiteSpace(GroupId);

    public override string ToString()
    {
        var prefix = IsMultiKey ? "*MK* " : String.Empty;
        var suffix = IsMultiKey ? $" [{GroupId}]" : String.Empty;
        return $"{prefix}[{PrimarySchemaName}].[{PrimaryTableName}].{PrimaryColumnNames} ({CascadeDeleteRule}){suffix}";
    }

    public string GetForeignKeyName()
    {
        return NameHelper.GetForeignKeyName(
            ForeignTableName,
            ForeignColumnNames,
            PrimaryTableName,
            PrimaryColumnNames);
    }
}