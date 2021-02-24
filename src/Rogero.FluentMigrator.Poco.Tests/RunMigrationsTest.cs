using System;
using System.Threading.Tasks;
using UniqueDb.ConnectionProvider;
using Xunit;
using Xunit.Abstractions;

namespace Rogero.FluentMigrator.Poco.Tests.Runner
{
    public class RunMigrationsTest : UnitTestBaseWithConsoleRedirection
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

        public RunMigrationsTest(ITestOutputHelper outputHelperHelper) : base(outputHelperHelper)
        {
            _scp = new UniqueDbConnectionProvider(new UniqueDbConnectionProviderOptions(
                                                      "WS2016Sql",
                                                      "POCO_Migration"));
        }
    }
}