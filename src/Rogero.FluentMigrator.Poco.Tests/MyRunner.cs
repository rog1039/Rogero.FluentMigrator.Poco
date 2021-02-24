using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions.Formatting;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Rogero.FluentMigrator.Poco.Tests.RelationalTypes;
using Rogero.FluentMigrator.Poco.Tests.Runner;
using UniqueDb.ConnectionProvider;
using UniqueDb.ConnectionProvider.DataGeneration;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class MyDecimalSqlTypeAttribute : DecimalTypeAttribute
    {
        public MyDecimalSqlTypeAttribute() : base(38, 12) { }
    }
    
    [TableName("Sales.Order")]
    public record Order2(
        [property: PrimaryKey] [property: Identity]
        int OrderNumber,
        [property: StringType(200)]
        string CustomerName,
        [property: StringType(100)]
        string? PONumber
    );

    public record OrderLine2(
        [property: PrimaryKey] [property: ForeignKeyRef(typeof(Order2), Rule.Cascade, nameof(Order2.OrderNumber))]
        int OrderNumber,
        [property: PrimaryKey]
        int OrderLineNumber,
        [property: ForeignKeyRef(typeof(Part2), Rule.Cascade, nameof(Part2.PartNumber))] [property: StringType(50)]
        string PartNumber,

        [property: MyDecimalSqlType]
        decimal Quantity
    );

    public class OrderRelease2
    {
        [PrimaryKey()]
        [ForeignKeyRef(typeof(OrderLine2), Rule.Cascade, nameof(Order2.OrderNumber), foreignKeyGroupId:"fk_OrderLine")]
        public int OrderNumber { get; }

        [PrimaryKey]
        [ForeignKeyRef(typeof(OrderLine2), Rule.Cascade, "OrderLineNumber", "fk_OrderLine")]
        public int OrderLineNumber { get; set; }
        
        [PrimaryKey]
        public int OrderReleaseNumber { get; set; }

        public DateTime ShipDate { get; set; }

        [MyDecimalSqlType]
        public decimal Quantity { get; set; }
    }

    [TableName("Inventory.Part2")]
    public class Part2
    {
        [PrimaryKey()]
        [StringType(50)]
        public string PartNumber { get; set; }

        [StringType(Length = 50)]
        public string PartDescription { get; set; }
        
        [RowVersionType]
        public byte[] RowVersion { get; set; }
    }

    [Migration(1)]
    [Tags("Group1")]
    public class MyMigration1 : Migration
    {
        public override void Up()
        {
            var types   = new List<Type>() { typeof(Part2),typeof(Order2), typeof(OrderLine2), typeof(OrderRelease2)};
            var configs = types.Select(TableDataFactory.GetTableCreationData);
            foreach (var creationData in configs)
            {
                this.Apply(creationData);
            }
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }

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
                        .ScanIn(typeof(MyRunnerTests).Assembly)
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