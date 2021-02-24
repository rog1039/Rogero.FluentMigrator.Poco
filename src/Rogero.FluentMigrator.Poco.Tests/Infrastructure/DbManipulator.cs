using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Rogero.FluentMigrator.Poco.Tests.Runner;
using UniqueDb.ConnectionProvider;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class DbManipulator
    {
        public           IList<string>          Tags { get; set; }
        private readonly ISqlConnectionProvider _sqlConnectionProvider;
        private readonly IServiceProvider       _serviceProvider;

        public bool Log { get; set; }

        public DbManipulator(ISqlConnectionProvider sqlConnectionProvider, IEnumerable<string> tags = null)
        {
            _sqlConnectionProvider = sqlConnectionProvider;
            _serviceProvider       = ConfigureOptionsAndCreateServiceProvider();

            Tags = new List<string>(tags ?? Enumerable.Empty<string>());
        }

        private IServiceProvider ConfigureOptionsAndCreateServiceProvider()
        {
            return new ServiceCollection()

                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb =>
                {
                    rb
                        // Add SQLite support to FluentMigrator
                        .AddSqlServer2016()

                        // Set the connection string
                        .WithGlobalConnectionString(_sqlConnectionProvider.GetSqlConnectionString())

                        // Define the assembly containing the migrations
                        .ScanIn(typeof(RunMigrationsTest).Assembly)
                        .For.Migrations()
                        .For.EmbeddedResources();
                })

                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .Configure<RunnerOptions>(ro =>
                {
                    /*
                     * This is where different migrations can be selected based on their tags.
                     * ro.Tags = new[] { "UK", "Production" }
                     */
                    ro.Tags = Tags.ToArray();
                })

                // Build the service provider
                .BuildServiceProvider(false);
        }

        public async Task CreateDatabase()
        {
            _sqlConnectionProvider.EnsureDatabaseExists();
        }

        public async Task CreateAndUpdateDatabase()
        {
            await CreateDatabase();
            await UpdateDatabase();
        }

        public async Task UpdateDatabase()
        {
            var runner = _serviceProvider.GetService<IMigrationRunner>();
            runner.ListMigrations();
            runner.MigrateUp();
        }

        public async Task UpradeTo(long migrationNumber)
        {
            var runner = _serviceProvider.GetService<IMigrationRunner>();
            runner.MigrateUp(migrationNumber);
        }

        public async Task DowngradeTo(long migrationNumber)
        {
            var runner = _serviceProvider.GetService<IMigrationRunner>();
            runner.MigrateDown(migrationNumber);
        }

        public async Task DeleteDatabase()
        {
            DatabaseDeleter.DeleteDatabase(_sqlConnectionProvider);
        }

        public async Task ListMigrations()
        {
            var runner = _serviceProvider.GetService<IMigrationRunner>();
            runner.ListMigrations();
        }
    }
}