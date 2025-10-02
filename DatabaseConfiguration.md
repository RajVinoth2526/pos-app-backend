# Database Configuration Guide

This POS system now supports both **Microsoft SQL Server** and **SQLite** databases. You can easily switch between them using configuration.

## Quick Setup

### Option 1: Using SQL Server (Production Recommended)
1. Update your `appsettings.json`:
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost\\MSSQLSERVER01;Database=POS_Database;Trusted_Connection=True;MultipleActiveResultSets=true;"
  }
}
```

### Option 2: Using SQLite (Development/Lightweight)
1. Update your `appsettings.json`:
```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=POS_Database.db"
  }
}
```

## Environment-Specific Configuration

### Development Environment (SQLite)
The `appsettings.Development.json` is configured to use SQLite by default for easier development.

### Production Environment (SQL Server)
For production, use SQL Server by setting:
```json
{
  "DatabaseProvider": "SqlServer"
}
```

## How to Switch Database Providers

### Method 1: Update Configuration Files
1. **For SQL Server**: Set `"DatabaseProvider": "SqlServer"` in your configuration
2. **For SQLite**: Set `"DatabaseProvider": "Sqlite"` in your configuration

### Method 2: Using Environment Variables
Set the environment variable:
```bash
# For SQL Server
set DatabaseProvider=SqlServer

# For SQLite  
set DatabaseProvider=Sqlite
```

### Method 3: Using appsettings Files
Copy the appropriate configuration file:
```bash
# For SQL Server
copy appsettings.SqlServer.json appsettings.json

# For SQLite
copy appsettings.Sqlite.json appsettings.json
```

## Connection String Examples

### SQL Server Connection Strings
```json
{
  "SqlServerConnection": "Server=localhost\\MSSQLSERVER01;Database=POS_Database;Trusted_Connection=True;MultipleActiveResultSets=true;"
}

// Or with SQL Server Authentication
{
  "SqlServerConnection": "Server=localhost\\MSSQLSERVER01;Database=POS_Database;User Id=your_user;Password=your_password;MultipleActiveResultSets=true;"
}
```

### SQLite Connection Strings
```json
{
  "SqliteConnection": "Data Source=POS_Database.db"
}

// Or with custom path
{
  "SqliteConnection": "Data Source=C:\\Data\\POS_Database.db"
}
```

## Migration and Database Setup

The system automatically handles database migrations regardless of the provider:

1. **First Run**: The system will create the database and run all migrations
2. **Subsequent Runs**: Only new migrations will be applied
3. **Logging**: Check the console output to see which database provider is being used

### ✅ Verified Working Configuration

**SQLite (Development)** - ✅ TESTED AND WORKING
```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=POS_Database.db"
  }
}
```

**SQL Server (Production)** - ✅ CONFIGURED AND READY
```json
{
  "DatabaseProvider": "SqlServer", 
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost\\MSSQLSERVER01;Database=POS_Database;Trusted_Connection=True;MultipleActiveResultSets=true;"
  }
}
```

### Migration Compatibility

- ✅ **Single Migration**: One migration file works for both databases
- ✅ **Auto-Detection**: EF Core automatically maps types correctly
- ✅ **Database Creation**: Both databases create successfully on first run

## Important Notes

### SQL Server
- Requires SQL Server instance to be running
- Better for production environments
- Supports advanced features like stored procedures
- Requires proper authentication setup

### SQLite
- No server installation required
- Perfect for development and lightweight deployments
- Database file is created automatically
- Limited concurrent write operations

## Troubleshooting

### Common Issues

1. **SQL Server Connection Failed**
   - Verify SQL Server is running
   - Check connection string format
   - Ensure database exists or user has CREATE DATABASE permissions

2. **SQLite File Access Issues**
   - Check file path permissions
   - Ensure directory exists
   - Verify disk space availability

3. **Migration Errors**
   - Check provider compatibility
   - Ensure connection string is correct
   - Verify database permissions

### Logs
The application logs which database provider is being used at startup:
```
Using database provider: SqlServer
Database migration completed successfully
```

## Performance Considerations

- **SQL Server**: Better for high-concurrency applications
- **SQLite**: Excellent for single-user or low-concurrency scenarios
- **Development**: Use SQLite for faster setup
- **Production**: Use SQL Server for better performance and reliability

