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
        public int?    OrderNumber { get; set; }
        public string? Description        { get; set; }
        public double Number        { get; set; }
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
                
                columns[0].SqlTypeAttribute.AllowNull.Should().BeTrue();
                columns[0].SqlTypeAttribute.Should().BeOfType(typeof(Int32TypeAttribute));
                
                columns[1].SqlTypeAttribute.AllowNull.Should().BeTrue();
                columns[1].SqlTypeAttribute.Should().BeOfType(typeof(StringTypeAttribute));

                columns[2].SqlTypeAttribute.AllowNull.Should().BeFalse();
                columns[2].SqlTypeAttribute.Should().BeOfType(typeof(DoubleTypeAttribute));


            }
    
            public NullableTests(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper) { }
        }
}