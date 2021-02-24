using System;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class TableNameAttribute : Attribute
    {
        public string TableName  { get; }
        public string SchemaName { get; }

        public TableNameAttribute(string tableName, string schemaName = "dbo")
        {
            TableName  = tableName;
            SchemaName = schemaName;
        }
    }
}