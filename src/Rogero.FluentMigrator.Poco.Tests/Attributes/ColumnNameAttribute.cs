using System;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class ColumnNameAttribute : Attribute
    {
        public string ColumnName { get; }

        public ColumnNameAttribute(string ColumnName)
        {
            ColumnName = ColumnName;
        }
    }
}