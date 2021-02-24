using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public record ForeignKeyInformation(
        string        ForeignSchemaName,
        string        ForeignTableName,
        IList<string> ForeignColumnNames,
        string        PrimarySchemaName,
        string        PrimaryTableName,
        IList<string> PrimaryColumnNames,
        Rule          CascadeDeleteRule)
    {
        public string GroupId { get; set; } = String.Empty;

        public override string ToString()
        {
            return $"{PrimarySchemaName}.{PrimaryTableName}.{PrimaryColumnNames.Single()} ({CascadeDeleteRule})";
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