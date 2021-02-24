using System;
using System.Threading.Tasks;
using FluentMigrator;
using FluentMigrator.SqlServer;
using UniqueDb.ConnectionProvider;
using Xunit;
using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests.Runner
{
    public class MyRunnerTests : UnitTestBaseWithConsoleRedirection
    {
        private UniqueDbConnectionProvider? _scp;

        [Fact()]
        [Trait("Category", "Instant")]
        public async Task MigrateGroup1()
        {
            var dbManipulator = new DbManipulator(_scp, new[]{"Group1"});
            await dbManipulator.CreateAndUpdateDatabase();
        }

        [Fact()]
        [Trait("Category", "Instant")]
        public async Task MigrateGroup2()
        {
            var dbManipulator = new DbManipulator(_scp, new []{"Group2"});
            await dbManipulator.CreateAndUpdateDatabase();
        }

        [Fact()]
        [Trait("Category", "Instant")]
        public void DeleteOlderDatabases()
        {
            OldDatabaseDeleter.DeleteOldDatabases(_scp, TimeSpan.FromMinutes(3));
        }

        public MyRunnerTests(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper)
        {
            _scp = new UniqueDbConnectionProvider(new UniqueDbConnectionProviderOptions(
                                                      "WS2016Sql",
                                                      "POCO_Migration"));
        }
    }

    [Migration(2)]
    [Tags("Group2")]
    public class MigrationOutOfOrder : Migration
    {
        public override void Up()
        {
            Create.Table("Order")
                .WithColumn("OrderNumber").AsInt32().PrimaryKey().NotNullable().Identity(1, 1)
                ;
            Create.Table("OrderLine")
                .WithColumn("OrderNumber").AsInt32().PrimaryKey().NotNullable()
                .ForeignKey("Order", "OrderNumber");
                ;
                
        }

        public override void Down()
        {
            throw new System.NotImplementedException();
        }
    }
}