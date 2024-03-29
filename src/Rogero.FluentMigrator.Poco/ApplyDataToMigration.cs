﻿using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.SqlServer;
using Rogero.FluentMigrator.Poco.RelationalTypes;

namespace Rogero.FluentMigrator.Poco;

public class DbModelApplicationOptions
{
    public bool ApplyForeignKeys { get; set; } = true;
}

public static class ApplyDataToMigration
{
    public static void Apply(this Migration            migration, DbModel dbModel,
                             DbModelApplicationOptions options = null)
    {
        options ??= new();
        foreach (var outputTableData in dbModel.OutputTableDatas)
        {
            migration.Apply(outputTableData, options);
        }
    }

    public static void Apply(this Migration            migration, 
                             TableData tableData,
                             DbModelApplicationOptions options = null)
    {
        options ??= new ();
        var (table, columns) = tableData;
        var tableExp = migration.Create.Table(table.Table);

        if (!string.IsNullOrWhiteSpace(table.Schema)) tableExp.InSchema(table.Schema);

        foreach (var column in columns)
        {
            var colExp  = tableExp.WithColumn(column.ColumnDataName.Name);
            var colExp2 = ApplyColumnType(column, colExp);

            if (column.PrimaryKeyInformation is { IsPrimaryKey: true })
            {
                colExp2.PrimaryKey();
            }

            if (column.IdentityInformation is var (seed, increment))
            {
                colExp2.Identity(seed, increment);
            }

            if (column.ForeignKeyInformation is { IsMultiKey: false } fk 
             && options.ApplyForeignKeys)
            {
                colExp2
                    .ForeignKey(fk.GetForeignKeyName(),
                                fk.PrimarySchemaName,
                                fk.PrimaryTableName,
                                fk.PrimaryColumnNames
                    )
                    .OnDelete(fk.CascadeDeleteRule);
            }
        }

        if (options.ApplyForeignKeys)
        {
            foreach (var fk in tableData.MultiForeignKeys)
            {
                migration.Create.ForeignKey(fk.GetForeignKeyName())
                    .FromTable(fk.ForeignTableName).InSchema(fk.ForeignSchemaName).ForeignColumns(fk.ForeignColumnNames.ToArray())
                    .ToTable(fk.PrimaryTableName).InSchema(fk.PrimarySchemaName).PrimaryColumns(fk.PrimaryColumnNames.ToArray());
            }
        }
    }

    public static ICreateTableColumnOptionOrWithColumnSyntax ApplyColumnType(
        ColumnData column, ICreateTableColumnAsTypeSyntax exp)
    {
        if (column.SqlTypeAttribute == null) throw new InvalidOperationException("Column does not have SqlType");

        var exp2 = column.SqlTypeAttribute switch
        {
            BinaryTypeAttribute binaryTypeAttribute       => exp.AsBinary(binaryTypeAttribute.Length),
            BoolTypeAttribute boolTypeAttribute           => exp.AsBoolean(),
            ByteTypeAttribute byteTypeAttribute           => exp.AsByte(),
            CurrencyTypeAttribute currencyTypeAttribute   => exp.AsCurrency(),
            CustomTypeAttribute customTypeAttribute       => exp.AsCustom(customTypeAttribute.CustomName),
            DateTime2TypeAttribute dateTime2TypeAttribute => exp.AsDateTime2(),
            DateTimeOffsetTypeAttribute dateTimeOffset    => exp.AsDateTimeOffset(),
            DecimalTypeAttribute decimalType              => exp.AsDecimal(decimalType.Precision, decimalType.Scale),
            DoubleTypeAttribute doubleTypeAttribute       => exp.AsDouble(),
            FloatTypeAttribute floatTypeAttribute         => exp.AsFloat(),
            GuidTypeAttribute guidTypeAttribute           => exp.AsGuid(),
            Int16TypeAttribute int16TypeAttribute         => exp.AsInt16(),
            Int32TypeAttribute int32TypeAttribute         => exp.AsInt32(),
            Int64TypeAttribute int64TypeAttribute         => exp.AsInt64(),
            RowVersionTypeAttribute rowVersionType        => exp.AsCustom("rowversion"),
            StringTypeAttribute stringTypeAttribute       => ApplyStringAttribute(exp, stringTypeAttribute),
            TimeTypeAttribute timeTypeAttribute           => exp.AsTime(),
            XmlTypeAttribute xmlTypeAttribute             => exp.AsXml(),
            _ => throw new ArgumentOutOfRangeException(
                $"No match for: {column.SqlTypeAttribute.GetType().FullName}")
        };
        var exp3 = column.SqlTypeAttribute.AllowNull
            ? exp2.Nullable()
            : exp2.NotNullable();
        return exp3;
    }

    private static ICreateTableColumnOptionOrWithColumnSyntax ApplyStringAttribute(
        ICreateTableColumnAsTypeSyntax exp, StringTypeAttribute stringTypeAttribute)
    {
        return (stringTypeAttribute.IsAnsi, FixedLength: stringTypeAttribute.IsFixedLength) switch
        {
            // IsAnsi , FixedLength
            (true, true)   => exp.AsFixedLengthAnsiString(stringTypeAttribute.Length),
            (true, false)  => exp.AsAnsiString(stringTypeAttribute.Length),
            (false, true)  => exp.AsFixedLengthString(stringTypeAttribute.Length),
            (false, false) => exp.AsString(stringTypeAttribute.Length),
        };
    }
}