using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using Rogero.FluentMigrator.Poco.Attributes;
using Rogero.FluentMigrator.Poco.RelationalTypes;
using Xunit;
using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class Job
    {
        public int Id         { get; set; }
        [CascadeRule(Rule.None)]
        public int ResourceId { get; set; }
    }

    [TableName("Production.Resource", "MySchema")]
    public class Resource
    {
        public int    Id         { get; set; }
        public byte[] RowVersion { get; set; }
    }
    
    //Would like to have the primary key, identity gen, and foreign key's all inferred from the model above.


    public class Inferring_PocoObjects2_Tests : UnitTestBaseWithConsoleRedirection
    {
        [Fact]
        public void ConventionsTest()
        {
            var types = new List<Type>()
            {
                typeof(Job),
                typeof(Resource)
            };

            var dbModel = DbModelFactory.GenerateModel(types);
            
            var jobTable = dbModel.OutputTableDatas.Single(z => z.TableName.Table == "Job");
            var resourceTable = dbModel.OutputTableDatas.Single(z => z.SourceType.Name== "Resource");

            Console.WriteLine(jobTable.ToString());
            Console.WriteLine(resourceTable.ToString());

            jobTable.ColumnCreationData[0].ColumnDataName.Name.Should().Be("Id");
            jobTable.ColumnCreationData[0].PrimaryKeyInformation.IsPrimaryKey.Should().BeTrue();
            jobTable.ColumnCreationData[0].IdentityInformation.Seed.Should().Be(1);
            jobTable.ColumnCreationData[0].IdentityInformation.Increment.Should().Be(1);

            var resourceIdColumn = jobTable.ColumnCreationData[1];
            resourceIdColumn.ForeignKeyInformation.ForeignSchemaName.Should().Be("dbo");
            resourceIdColumn.ForeignKeyInformation.ForeignTableName.Should().Be("Job");
            resourceIdColumn.ForeignKeyInformation.ForeignColumnNames.Should().Be("ResourceId");
            resourceIdColumn.ForeignKeyInformation.PrimarySchemaName.Should().Be("MySchema");
            resourceIdColumn.ForeignKeyInformation.PrimaryTableName.Should().Be("Production.Resource");
            resourceIdColumn.ForeignKeyInformation.PrimaryColumnNames.Should().Be("Id");
            
            jobTable.ColumnCreationData[0].ColumnDataName.Name.Should().Be("Id");
            jobTable.ColumnCreationData[0].PrimaryKeyInformation.IsPrimaryKey.Should().BeTrue();
            jobTable.ColumnCreationData[0].IdentityInformation.Seed.Should().Be(1);
            jobTable.ColumnCreationData[0].IdentityInformation.Increment.Should().Be(1);
            
            
        }

        public Inferring_PocoObjects2_Tests(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper) { }
    }
}