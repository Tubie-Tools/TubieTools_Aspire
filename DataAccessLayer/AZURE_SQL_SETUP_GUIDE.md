# Azure SQL Database Configuration Guide

## Overview
This guide explains how to configure your TubieTools Aspire application to use Azure SQL Database for data persistence.

## Architecture

The solution uses multiple DbContexts:
- **TubieDbContext**: Main application database (CareProviders, Contacts, Addresses, etc.)
- **ApplicationDbContext**: ASP.NET Identity database (Users, Roles, etc.)
- **FoundryDbContext**: Foundry-related entities (optional)
- **MapAppDbContext**: Logistics/mapping data (optional)

## Configuration Files

### 1. SQL Azure Settings (`appsettings.AzureSql.json`)
Located in `DataAccessLayer/` directory, contains:
- Server endpoint (e.g., `your-server.database.windows.net`)
- Database names per context
- Connection timeout and retry settings
- Encryption and authentication options

### 2. Environment-Specific Configuration
Create per-environment configuration files:
- `appsettings.Development.json` (local/non-Azure)
- `appsettings.Staging.json` (staging)
- `appsettings.Production.json` (production with Azure SQL)

## Setup Steps

### Step 1: Create Azure SQL Server and Databases

```bash
# Set variables
RESOURCE_GROUP="tubie-rg"
SQL_SERVER="tubie-sql-server"
LOCATION="eastus"
ADMIN_USER="sqladmin"
ADMIN_PASSWORD="YourSecurePassword123!"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create SQL Server
az sql server create \
  --resource-group $RESOURCE_GROUP \
  --name $SQL_SERVER \
  --admin-user $ADMIN_USER \
  --admin-password $ADMIN_PASSWORD \
  --location $LOCATION

# Create databases
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name TubieDb

az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name TubieIdentityDb
```

### Step 2: Configure Firewall Rules

```bash
# Get your client IP
CLIENT_IP=$(curl -s https://api.ipify.org)

# Add firewall rule to allow your machine
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowClientIP \
  --start-ip-address $CLIENT_IP \
  --end-ip-address $CLIENT_IP

# For App Service (if deploying to Azure)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Step 3: Update API Configuration

In your API's `Program.cs` (e.g., `TubieTools_PublicAPI/Program.cs`):

```csharp
using DataAccessLayer.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add SQL Azure DbContexts
builder.Services.AddTubieDbContextSqlAzure(builder.Configuration);
builder.Services.AddApplicationDbContextSqlAzure(builder.Configuration);

// Or add all contexts at once
// builder.Services.AddMultipleDbContextsSqlAzure(builder.Configuration);

var app = builder.Build();
app.Run();
```

### Step 4: Update appsettings.json

Replace placeholder values in your API's `appsettings.json`:

```json
{
  "SqlAzure": {
	"Server": "tubie-sql-server.database.windows.net",
	"Database": "TubieDb",
	"UserId": "sqladmin",
	"Password": "${SQL_PASSWORD}",
	"ConnectTimeout": 30,
	"CommandTimeout": 300,
	"EnableRetryOnFailure": true,
	"MaxRetryCount": 3,
	"MaxRetryDelay": 5,
	"Encrypt": true,
	"TrustServerCertificate": false
  },
  "SqlAzure:Identity": {
	"Server": "tubie-sql-server.database.windows.net",
	"Database": "TubieIdentityDb",
	"UserId": "sqladmin",
	"Password": "${SQL_PASSWORD}"
  }
}
```

### Step 5: Apply Entity Framework Migrations

```bash
# Navigate to your API project
cd TubieTools_PublicAPI

# Apply migrations to TubieDb
dotnet ef database update --context TubieDbContext \
  --connection "Server=tubie-sql-server.database.windows.net;Database=TubieDb;User Id=sqladmin;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;"

# Apply migrations to TubieIdentityDb
dotnet ef database update --context ApplicationDbContext \
  --connection "Server=tubie-sql-server.database.windows.net;Database=TubieIdentityDb;User Id=sqladmin;Password=YourPassword;Encrypt=true;TrustServerCertificate=false;"
```

### Step 6: Manage Secrets Securely

**Never commit passwords to source control!**

#### Local Development (User Secrets):
```bash
cd TubieTools_PublicAPI
dotnet user-secrets init
dotnet user-secrets set "SqlAzure:Password" "YourSecurePassword123!"
```

#### Azure Key Vault:
```bash
# Create Key Vault
az keyvault create --resource-group $RESOURCE_GROUP --name tubie-kv

# Add secret
az keyvault secret set --vault-name tubie-kv --name SqlPassword --value "YourSecurePassword123!"

# Grant App Service/Container access
az keyvault set-policy --name tubie-kv --object-id <app-identity-id> --secret-permissions get list
```

#### Environment Variables:
Set in deployment configuration:
```bash
export SQL_PASSWORD="YourSecurePassword123!"
```

### Step 7: Test Connection

Use the `IAzureSqlConnectionManager` to test:

```csharp
public class SqlConnectionTest
{
	public async Task TestAsync(IAzureSqlConnectionManager connectionManager)
	{
		try
		{
			var isConnected = await connectionManager.TestConnectionAsync();
			if (isConnected)
			{
				Console.WriteLine("✓ Successfully connected to Azure SQL");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"✗ Connection failed: {ex.Message}");
		}
	}
}
```

## Dependency Injection Registration

The configuration provides extension methods for easy DI registration:

```csharp
// Register a single DbContext
services.AddTubieDbContextSqlAzure(configuration);

// Register Application Identity context
services.AddApplicationDbContextSqlAzure(configuration);

// Register multiple contexts
services.AddMultipleDbContextsSqlAzure(configuration);

// Register connection manager
services.AddScoped<IAzureSqlConnectionManager>(sp => 
	new AzureSqlConnectionManager(
		sp.GetRequiredService<ILogger<AzureSqlConnectionManager>>(),
		configuration.GetSection("SqlAzure").Get<SqlAzureConfiguration.SqlAzureSettings>()!
			.GetConnectionString()
	)
);

// Register database initializer
services.AddScoped<IAzureDatabaseInitializer, AzureDatabaseInitializer>();
```

## Connection String Format

The connection string follows Azure SQL requirements:

```
Server=tcp:server-name.database.windows.net,1433;
Initial Catalog=database-name;
Persist Security Info=False;
User ID=username;
Password=password;
MultipleActiveResultSets=False;
Encrypt=True;
Connection Timeout=30;
TrustServerCertificate=False;
Command Timeout=300;
```

## Retry and Failover Policies

The configuration includes automatic retry logic:
- **Max Retry Count**: 3 attempts
- **Retry Delay**: 5 seconds (first attempt)
- **Transient Error Handling**: Automatically retries on timeouts and connection failures
- **Query Splitting**: Uses `QuerySplittingBehavior.SplitQuery` for better performance

## Monitoring and Logging

Enable detailed logging in development:

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft": "Warning",
	  "Microsoft.EntityFrameworkCore": "Debug",
	  "Microsoft.EntityFrameworkCore.Database.Command": "Information"
	}
  }
}
```

## Troubleshooting

### Connection Timeout
- Check firewall rules are configured correctly
- Verify SQL Server is running and accessible
- Increase `ConnectTimeout` in settings
- Check credentials are correct

### Authentication Failed
- Verify username and password
- Ensure SQL Server has firewall exception for your IP
- Check that the user has appropriate database access

### Database Does Not Exist
- Verify database name matches configuration
- Ensure database was created (see Step 1)
- Check connection string database parameter

### Migrations Not Applied
- Run `dotnet ef migrations list` to see pending migrations
- Run `dotnet ef database update` to apply all pending migrations
- Verify user has ALTER DATABASE permissions

## Performance Tuning

### Connection Pooling
SQL Client automatically pools connections. Configuration:
- Default minimum pool size: 0
- Default maximum pool size: 100
- Adjust in connection string if needed

### Query Performance
- Use `QuerySplittingBehavior.SplitQuery` for large result sets (enabled by default)
- Enable EF Core query logging to identify N+1 queries
- Create appropriate database indexes for frequently searched columns

## Disaster Recovery

### Backup and Restore
```bash
# List current backups
az sql db show --resource-group $RESOURCE_GROUP --server $SQL_SERVER --name TubieDb

# Point-in-time restore
az sql db restore --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name TubieDb-restored \
  --from-name TubieDb \
  --point-in-time "2024-01-15T10:00:00"
```

### Geo-Replication
```bash
# Create geo-replicated secondary database
az sql db replica create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name TubieDb \
  --partner-server tubie-sql-server-secondary \
  --partner-resource-group $RESOURCE_GROUP
```

## Cost Optimization

1. **SKU Selection**: Start with Standard tier; upgrade if needed
2. **Auto-pause**: Enable on dev/test databases with `--auto-pause-delay` 
3. **Reserved Capacity**: Use reserved instances for predictable workloads
4. **Backup Retention**: Adjust backup retention policy (default 7 days)

## Next Steps

1. Create Azure SQL Server and databases
2. Update API `Program.cs` files with DbContext registration
3. Add connection string to `appsettings.json` (with secrets management)
4. Apply Entity Framework migrations
5. Test connectivity using `IAzureSqlConnectionManager`
6. Deploy to Azure App Service or Container Apps

## References

- [Azure SQL Database Documentation](https://learn.microsoft.com/azure/azure-sql/)
- [Entity Framework Core SQL Server Provider](https://learn.microsoft.com/ef/core/providers/sql-server/)
- [Aspire Multi-tenancy with SQL Database](https://learn.microsoft.com/dotnet/aspire/deployment/manifest-format)
- [Azure Key Vault Integration](https://learn.microsoft.com/azure/key-vault/)
