using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ClientAppPOSWebAPI.Configuration
{
    public static class DatabaseConfiguration
    {
        public enum DatabaseProvider
        {
            SqlServer,
            Sqlite
        }

        public static void ConfigureDatabase(this DbContextOptionsBuilder options, IConfiguration configuration)
        {
            var provider = configuration["DatabaseProvider"]?.ToString() ?? "SqlServer";
            
            switch (provider.ToLower())
            {
                case "sqlite":
                    ConfigureSqlite(options, configuration);
                    break;
                case "sqlserver":
                default:
                    ConfigureSqlServer(options, configuration);
                    break;
            }
        }

        private static void ConfigureSqlServer(DbContextOptionsBuilder options, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection");
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                // Map SQLite types to SQL Server types
                sqlOptions.CommandTimeout(30);
            });
        }

        private static void ConfigureSqlite(DbContextOptionsBuilder options, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqliteConnection");
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                // Ensure SQLite compatibility
                sqliteOptions.CommandTimeout(30);
            });
        }

        public static DatabaseProvider GetDatabaseProvider(IConfiguration configuration)
        {
            var provider = configuration["DatabaseProvider"]?.ToString() ?? "SqlServer";
            
            return provider.ToLower() switch
            {
                "sqlite" => DatabaseProvider.Sqlite,
                "sqlserver" or _ => DatabaseProvider.SqlServer
            };
        }

        public static string GetConnectionString(IConfiguration configuration)
        {
            var provider = GetDatabaseProvider(configuration);
            
            return provider switch
            {
                DatabaseProvider.Sqlite => configuration.GetConnectionString("SqliteConnection") ?? "Data Source=POS_Database.db",
                DatabaseProvider.SqlServer => configuration.GetConnectionString("SqlServerConnection") ?? "Server=localhost\\MSSQLSERVER01;Database=POS_Database;Trusted_Connection=True;MultipleActiveResultSets=true;",
                _ => throw new InvalidOperationException($"Unsupported database provider: {provider}")
            };
        }
    }
}
