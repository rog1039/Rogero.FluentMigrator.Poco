using System;
using System.Collections.Generic;
using FluentAssertions;
using Rogero.FluentMigrator.Poco.RelationalTypes;
using Xunit;
using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class TypeWithNullables
    {
        public int?    OrderNumber    { get; set; }
        public string? Description    { get; set; }
        public double  Number         { get; set; }
        public double? NumberNullable { get; set; }
        public string  NotNullObject  { get; set; }
        public string? NullObject     { get; set; }
    }
    public class NullableTests : UnitTestBaseWithConsoleRedirection
    {
        [Fact]
        public void TestNullables()
        {
            var types = new List<Type>()
            {
                typeof(TypeWithNullables),
            };

            var dbModel = DbModelFactory.GenerateModel(types);

            Console.WriteLine(dbModel.ToString());

            var (table, columns) = dbModel.OutputTableDatas[0];

            columns[0].ColumnDataName.Name.Should().Be("OrderNumber");
            columns[0].SqlTypeAttribute.AllowNull.Should().BeTrue();
            columns[0].SqlTypeAttribute.Should().BeOfType(typeof(Int32TypeAttribute));

            columns[1].ColumnDataName.Name.Should().Be("Description");
            columns[1].SqlTypeAttribute.AllowNull.Should().BeTrue();
            columns[1].SqlTypeAttribute.Should().BeOfType(typeof(StringTypeAttribute));

            columns[2].ColumnDataName.Name.Should().Be("Number");
            columns[2].SqlTypeAttribute.AllowNull.Should().BeFalse();
            columns[2].SqlTypeAttribute.Should().BeOfType(typeof(DoubleTypeAttribute));

            columns[3].ColumnDataName.Name.Should().Be("NumberNullable");
            columns[3].SqlTypeAttribute.AllowNull.Should().BeTrue();
            columns[3].SqlTypeAttribute.Should().BeOfType(typeof(DoubleTypeAttribute));

            columns[4].ColumnDataName.Name.Should().Be("NotNullObject");
            columns[4].SqlTypeAttribute.AllowNull.Should().BeFalse();
            columns[4].SqlTypeAttribute.Should().BeOfType(typeof(StringTypeAttribute));

            columns[5].ColumnDataName.Name.Should().Be("NullObject");
            columns[5].SqlTypeAttribute.AllowNull.Should().BeTrue();
            columns[5].SqlTypeAttribute.Should().BeOfType(typeof(StringTypeAttribute));
        }

        public NullableTests(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper) { }
    }
}