using System;
using System.Linq;
using FluentMigrator.Infrastructure.Extensions;
using Rogero.FluentMigrator.Poco.Attributes;

namespace Rogero.FluentMigrator.Poco
{
    public static class TableDataFactory
    {
        public static TableData CreateTableDataFromType(Type type)
        {
            var tableName         = GetTableName(type);
            var tableCreationData = new TableData() {TableName = tableName, SourceType=type};
            var columnCreationDatas = type
                .GetBasePropertiesFirst()
                .Select(prop => ColumnDataFactory.GetInfo(tableCreationData, prop))
                .Where(z => z is not null)
                .ToList();
            tableCreationData.ColumnCreationData.AddRange(columnCreationDatas);
            tableCreationData.BuildMultiForeignKeys();
            return tableCreationData;
        }

        public static SchemaTableNames GetTableName(Type type)
        {
            var tableNameAttribute = type.GetOneAttribute<TableNameAttribute>();
            return tableNameAttribute != null
                ? new SchemaTableNames(tableNameAttribute.SchemaName, tableNameAttribute.TableName)
                : new SchemaTableNames("dbo",                         type.Name);
        }
    }
}