using ClientAppPOSWebAPI.Data;
using ClientAppPOSWebAPI.Models;
using ClientAppPOSWebAPI.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ClientAppPOSWebAPI.Controllers
{
    [Route("api/database")]
    [ApiController]
    [Authorize(Roles = "Admin,admin")]
    public class DatabaseController : ControllerBase
    {
        private readonly POSDbContext _context;
        private readonly IConfiguration _configuration;

        public DatabaseController(POSDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/database/info
        [HttpGet("info")]
        public IActionResult GetDatabaseInfo()
        {
            try
            {
                var provider = DatabaseConfiguration.GetDatabaseProvider(_configuration);
                var connectionString = DatabaseConfiguration.GetConnectionString(_configuration);

                var info = new
                {
                    Provider = provider.ToString(),
                    ConnectionString = GetMaskedConnectionString(connectionString),
                    DatabaseName = GetDatabaseName(provider, connectionString),
                    ProviderName = _context.Database.ProviderName,
                    CanConnect = CanConnectToDatabase()
                };

                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = $"Database configuration error: {ex.Message}" });
            }
        }

        // GET: api/database/test-connection
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();
                
                return Ok(new { 
                    success = true, 
                    message = "Database connection successful",
                    provider = _context.Database.ProviderName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Database connection failed",
                    error = ex.Message
                });
            }
        }

        // GET: api/database/migrate
        [HttpPost("migrate")]
        public async Task<IActionResult> RunMigrations()
        {
            try
            {
                await _context.Database.MigrateAsync();
                
                return Ok(new { 
                    success = true, 
                    message = "Database migrations completed successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Database migration failed",
                    error = ex.Message
                });
            }
        }

        private string GetMaskedConnectionString(string connectionString)
        {
            // Mask sensitive information in connection string
            if (connectionString.Contains("Password="))
            {
                var startIndex = connectionString.IndexOf("Password=");
                var endIndex = connectionString.IndexOf(";", startIndex);
                if (endIndex == -1) endIndex = connectionString.Length;
                
                var passwordPart = connectionString.Substring(startIndex, endIndex - startIndex);
                connectionString = connectionString.Replace(passwordPart, "Password=***");
            }
            
            return connectionString;
        }

        private string GetDatabaseName(DatabaseConfiguration.DatabaseProvider provider, string connectionString)
        {
            return provider switch
            {
                DatabaseConfiguration.DatabaseProvider.SqlServer => ExtractSqlServerDatabaseName(connectionString),
                DatabaseConfiguration.DatabaseProvider.Sqlite => ExtractSqliteDatabaseName(connectionString),
                _ => "Unknown"
            };
        }

        private string ExtractSqlServerDatabaseName(string connectionString)
        {
            var databaseIndex = connectionString.IndexOf("Database=");
            if (databaseIndex == -1) return "Unknown";
            
            var startIndex = databaseIndex + 9;
            var endIndex = connectionString.IndexOf(";", startIndex);
            if (endIndex == -1) endIndex = connectionString.Length;
            
            return connectionString.Substring(startIndex, endIndex - startIndex);
        }

        private string ExtractSqliteDatabaseName(string connectionString)
        {
            var dataSourceIndex = connectionString.IndexOf("Data Source=");
            if (dataSourceIndex == -1) return "Unknown";
            
            var startIndex = dataSourceIndex + 12;
            var endIndex = connectionString.IndexOf(";", startIndex);
            if (endIndex == -1) endIndex = connectionString.Length;
            
            return connectionString.Substring(startIndex, endIndex - startIndex);
        }

        private bool CanConnectToDatabase()
        {
            try
            {
                _context.Database.OpenConnection();
                _context.Database.CloseConnection();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
