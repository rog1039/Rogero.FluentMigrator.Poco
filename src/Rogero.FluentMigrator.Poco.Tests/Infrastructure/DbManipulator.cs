using System.Threading.Tasks;
using FluentMigrator.Runner;
using UniqueDb.ConnectionProvider;

namespace Rogero.FluentMigrator.Poco.Tests
{
    public class SqliteDbAdapter : IDbAdapter
    {
        private readonly string _connectionString;

        public SqliteDbAdapter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreateDatabase() { }

        public async Task DeleteDatabase() { }

        public IMigrationRunnerBuilder ConfigureRunner(IMigrationRunnerBuilder builder)
        {
            return builder
                .AddSQLite()
                .WithGlobalConnectionString(_connectionString);
        }
    }

    public class SqlServerDbAdapter : IDbAdapter
    {
        private readonly ISqlConnectionProvider _sqlConnectionProvider;

        public SqlServerDbAdapter(ISqlConnectionProvider sqlConnectionProvider)
        {
            _sqlConnectionProvider = sqlConnectionProvider;
        }

        public async Task CreateDatabase()
        {
            _sqlConnectionProvider.EnsureDatabaseExists();
        }

        public async Task DeleteDatabase()
        {
            DatabaseDeleter.DeleteDatabase(_sqlConnectionProvider);
        }

        public IMigrationRunnerBuilder ConfigureRunner(IMigrationRunnerBuilder builder)
        {
            return builder
                .AddSqlServer2016()
                .WithGlobalConnectionString(_sqlConnectionProvider?.GetSqlConnectionString());
        }
    }
}