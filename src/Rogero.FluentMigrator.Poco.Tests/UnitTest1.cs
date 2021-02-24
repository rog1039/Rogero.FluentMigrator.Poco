using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.SqlServer;
using Rogero.Common.ExtensionMethods;
using Xunit;
using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public record ColumnIdentityInformation(int Seed, int Increment);

    public record ColumnType();

    public record ColumnTypeDecimal(int Precision, int Scale) : ColumnType
    {
        public override string ToString()
        {
            return $"decimal({Precision}, {Scale})";
        }
    }

    public record ColumnTypeString(int? Length = int.MaxValue) : ColumnType
    {
        public override string ToString()
        {
            return Length > 0 && Length != int.MaxValue
                ? $"nvarchar({Length})"
                : "nvarchar(max)";
        }
    }

    public record ColumnTypeCustom(string SqlTypeDefinition) : ColumnType
    {
        public override string ToString()
        {
            return SqlTypeDefinition;
        }
    }

    public static class TableDataFactory
    {
        public static TableData GetTableCreationData(Type type)
        {
            var tableName         = GetTableName(type);
            var tableCreationData = new TableData() {TableName = tableName};
            var columnCreationDatas = type
                .GetProperties()
                .Select(prop => ColumnDataFactory.GetInfo(tableCreationData, prop))
                .ToList();
            tableCreationData.ColumnCreationData.AddRange(columnCreationDatas);
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


    public record SchemaTableNames(
        string Schema,
        string Table)
    {
        public override string ToString()
        {
            return $"{Schema}.{Table}";
        }
    };

    public class UnitTest1 : UnitTestBaseWithConsoleRedirection
    {
        [Fact]
        public void Test1()
        {
            var types = new List<Type>() {typeof(Order2), typeof(OrderLine2), typeof(OrderRelease2), typeof(Part2)};
            var tcds = types
                .Select(typ => TableDataFactory.GetTableCreationData(typ))
                .ToList();

            tcds.ForEach(t => Console.WriteLine(t.ToString()));

            tcds
                .PrintStringTable();
            tcds
                .ForEach(tcd => { tcd.ColumnCreationData.PrintStringTable(tcd.TableName.Table); });

            // tcds
            //     .SelectMany(z => z.ColumnCreationData)
            //     .PrintStringTable();

            // var tcd   = TableInfoRetriever.GetTableCreationData(typeof(Order));
            // new List<TableCreationData>() {tcd}.PrintStringTable();
            // tcd.ColumnCreationData.PrintStringTable();
        }

        public UnitTest1(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper) { }
    }

    public class TestOutputHelperToTextWriterAdapter : TextWriter
    {
        ITestOutputHelper _output;

        public TestOutputHelperToTextWriterAdapter(ITestOutputHelper output)
        {
            _output = output;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }

        public override void WriteLine(string message)
        {
            _output.WriteLine(message);
        }

        public override void WriteLine(string format, params object[] args)
        {
            _output.WriteLine(format, args);
        }

        public override void Write(char value) { }

        public override void Write(string message)
        {
            _output.WriteLine(message);
        }
    }

    public class UnitTestBaseWithConsoleRedirection
    {
        protected ITestOutputHelper _outputHelper;

        public UnitTestBaseWithConsoleRedirection(ITestOutputHelper outputHelperHelper)
        {
            Console.SetOut(new TestOutputHelperToTextWriterAdapter(outputHelperHelper));
            _outputHelper = outputHelperHelper;
        }
    }
}