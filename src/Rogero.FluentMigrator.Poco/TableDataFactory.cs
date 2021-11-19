using FluentMigrator.Infrastructure.Extensions;
using Rogero.Common.ExtensionMethods;
using Rogero.FluentMigrator.Poco.Attributes;

namespace Rogero.FluentMigrator.Poco;

public static class TableDataFactory
{
    public static TableData CreateTableDataFromType(Type type)
    {
        var tableName         = GetTableName(type);
        var tableCreationData = new TableData() {TableName = tableName, SourceType =type};
        var columnCreationDatas = type
            .GetBasePropertiesFirst()
            .Where(z => z.GetMethod?.IsStatic != true)
            .Select(prop => ColumnDataFactory.GetInfo(tableCreationData, prop))
            .WhereNotNull()
            .OrderBy(z => GetColumnOrder(z))
            .ToList();
        tableCreationData.ColumnCreationData.AddRange(columnCreationDatas);
        tableCreationData.BuildMultiForeignKeys();
        return tableCreationData;
    }

    private static int GetColumnOrder(ColumnData columnData)
    {
        return columnData.ColumnOrderInformation.Order;
    }

    public static SchemaTableNames GetTableName(Type type)
    {
        var tableNameAttribute = type.GetOneAttribute<TableNameAttribute>();
        return tableNameAttribute != null
            ? new SchemaTableNames(tableNameAttribute.SchemaName, tableNameAttribute.TableName)
            : new SchemaTableNames("dbo",                         type.Name);
    }
}

public class ColumnOrderAttribute : Attribute
{
    public int ColumnOrder { get; set; } = 0;

    public ColumnOrderAttribute(int columnOrder)
    {
        ColumnOrder = columnOrder;
    }
}