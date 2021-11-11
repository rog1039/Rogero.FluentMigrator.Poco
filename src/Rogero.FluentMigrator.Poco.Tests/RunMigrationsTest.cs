using FluentAssertions;
using Rogero.Common.ExtensionMethods;
using Rogero.FluentMigrator.Poco.Tests.Infrastructure;
using Rogero.FluentMigrator.Poco.Tests.Migrations;
using UniqueDb.ConnectionProvider;
using Xunit;
using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests;

public class RunMigrationsPreviewTest : UnitTestBaseWithConsoleRedirection
{
    [Fact()]
    [Trait("Category", "Instant")]
    public async Task MigrateGroup1Preview()
    {
        var model = DbModelFactory.GenerateModel(new Migration1().Types);
        model.OutputTableDatas.PrintStringTable("Tables in Topological Order");

        //Also, let's do a spot check on the Quantity columns MyDecimalSqlTypeAttribute data type.
        var orderLine2Table = model.OutputTableDatas.Single(z => z.TableName.Table == nameof(OrderLine2));
        var quantityColumn =
            orderLine2Table.ColumnCreationData.Single(z => z.ColumnDataName.Name == nameof(OrderLine2.Quantity));
        var myDecimalSqlTypeAttribute = quantityColumn.SqlTypeAttribute as MyDecimalSqlTypeAttribute;
        myDecimalSqlTypeAttribute.Precision.Should().Be(38);
        myDecimalSqlTypeAttribute.Scale.Should().Be(12);

        //And now let's print out the migration.
        var dbManipulator = new DbManipulator(new SqlServerDbAdapter(null),
                                              typeof(RunMigrationsTest).Assembly,
                                              new[] {"Group1"});
        dbManipulator.ShowSql         = true;
        dbManipulator.ShowElapsedTime = false;
        dbManipulator.PreviewOnly     = true;
        await dbManipulator.UpdateDatabase();
    }

    public RunMigrationsPreviewTest(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper) { }
}

public class RunMigrationsTest : UnitTestBaseWithConsoleRedirection
{
    private UniqueDbConnectionProvider? _scp;

    [Fact()]
    [Trait("Category", "Instant")]
    public async Task MigrateGroup1AgainstActualServer()
    {
        var dbManipulator = new DbManipulator(new SqlServerDbAdapter(_scp),
                                              typeof(RunMigrationsTest).Assembly,
                                              new[] {"Group1"});
        dbManipulator.ShowSql = true;
        await dbManipulator.CreateAndUpdateDatabase();
    }

    [Fact()]
    [Trait("Category", "Instant")]
    public async Task MigrateGroup2()
    {
        Func<Task> failCreatingDatabase = new Func<Task>(async () =>
        {
            var dbManipulator = new DbManipulator(new SqlServerDbAdapter(_scp),
                                                  typeof(RunMigrationsTest).Assembly,
                                                  new[] {"Group1"});
            await dbManipulator.CreateAndUpdateDatabase();
        });

        await failCreatingDatabase.Should()
            .ThrowAsync<Exception>(
                $"This fails because FluentMigrator doesn't sequence operations in appropriate order. {nameof(DbModelFactory)} can help topologically sort create statements to prevent this.");
    }

    [Fact()]
    [Trait("Category", "Instant")]
    public void DeleteOlderDatabases()
    {
        OldDatabaseDeleter.DeleteOldDatabases(_scp, TimeSpan.FromMinutes(3));
    }

    public RunMigrationsTest(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper)
    {
        _scp = new UniqueDbConnectionProvider(new UniqueDbConnectionProviderOptions(
                                                  "WS2016Sql",
                                                  "POCO_Migration"));
    }
}