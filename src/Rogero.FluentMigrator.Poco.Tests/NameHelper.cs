using System.Collections.Generic;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public static class NameHelper
    {
        public static string GetForeignKeyName(
            string        foreignTable,
            IList<string> foreignColumns,
            string        primaryTable,
            IList<string> primaryColumns
        )
        {
            var fkColumns = foreignColumns.StringJoin(",");
            var pkColumns = primaryColumns.StringJoin(",");
            return $"FK__{foreignTable}__{fkColumns}__TO__{primaryTable}__{pkColumns}";
        }
    }
}