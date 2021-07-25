using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;

namespace Rogero.FluentMigrator.Poco
{
    public interface IDbAdapter
    {
        Task                    CreateDatabase();
        Task                    DeleteDatabase();
        IMigrationRunnerBuilder ConfigureRunner(IMigrationRunnerBuilder builder);
    }

    public class DummyDbAdapter : IDbAdapter
    {
        public async Task CreateDatabase() { }

        public async Task DeleteDatabase() { }

        public IMigrationRunnerBuilder ConfigureRunner(IMigrationRunnerBuilder builder)
        {
            return builder;
        }
    }

    public class DbManipulator
    {
        public IList<string> Tags { get; set; }

        private          IServiceProvider _serviceProvider;
        private readonly IDbAdapter       _dbAdapter;
        private readonly Assembly         _migrationAssembly;

        public bool PreviewOnly     { get; set; } = false;
        public bool ShowElapsedTime { get; set; } = false;
        public bool ShowSql         { get; set; } = false;

        public DbManipulator(IDbAdapter      dbAdapter,
                             Assembly        migrationAssembly,
                             params string[] tags)
        {
            _dbAdapter         = dbAdapter;
            _migrationAssembly = migrationAssembly;
            Tags               = new List<string>(tags ?? Enumerable.Empty<string>());
            SetServiceProvider();
        }

        /// <summary>
        /// Can be called again to reconfigure the service provider. Useful if the Tags have been changed.
        /// </summary>
        public void SetServiceProvider()
        {
            _serviceProvider = ConfigureOptionsAndCreateServiceProvider();
        }

        private IServiceProvider ConfigureOptionsAndCreateServiceProvider()
        {
            return new ServiceCollection()

                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .Configure<FluentMigratorLoggerOptions>(
                    opt =>
                    {
                        opt.ShowElapsedTime = ShowElapsedTime;
                        opt.ShowSql         = ShowSql;
                    })
                .ConfigureRunner(rb =>
                {
                    rb = _dbAdapter.ConfigureRunner(rb);
                    rb
                        // Define the assembly containing the migrations
                        .ScanIn(_migrationAssembly)
                        .For.Migrations()
                        .For.EmbeddedResources()
                        .ConfigureGlobalProcessorOptions(opt => { opt.PreviewOnly = PreviewOnly; })
                        ;
                })

                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => { lb.AddFluentMigratorConsole(); })
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
            await _dbAdapter.CreateDatabase();
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
            await _dbAdapter.DeleteDatabase();
        }

        public async Task ListMigrations()
        {
            var runner = _serviceProvider.GetService<IMigrationRunner>();
            runner.ListMigrations();
        }
    }

    public enum SqlDbType
    {
        SqlServer,
        Sqlite
    }
}