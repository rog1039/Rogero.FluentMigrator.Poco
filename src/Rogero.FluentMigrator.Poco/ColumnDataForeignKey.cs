using System;
using System.Data;
using System.Linq;

namespace Rogero.FluentMigrator.Poco
{
    public record ColumnDataForeignKey(
        string ForeignSchemaName,
        string ForeignTableName,
        string ForeignColumnNames,
        string PrimarySchemaName,
        string PrimaryTableName,
        string PrimaryColumnNames,
        Rule   CascadeDeleteRule)
    {
        public string GroupId { get; set; } = String.Empty;

        public override string ToString()
        {
            return $"{PrimarySchemaName}.{PrimaryTableName}.{PrimaryColumnNames} ({CascadeDeleteRule})";
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
}