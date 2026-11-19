
using Dapper;
using FrontOfficeApplication.GlobalModels;
using FrontOfficeApplication.ProjectInfrastructure.DataBase;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace FrontOfficeApplication
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateMasterConnection();
        Task<IDbConnection> CreateConnection(string DatabaseType);
        Task<IDbConnection> CreateConnection(string branchCode, string DatabaseType);
        void ClearConnectionCache( string branchCode, string databaseType);
    }

    public class DBConnectionFactory : IDbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly ITenantContext _tenantContext;

        public DBConnectionFactory(IConfiguration configuration,
            IMemoryCache cache,
            ITenantContext tenantContext)
        {
            _configuration = configuration;
            _cache = cache;
            _tenantContext = tenantContext;
        }

        // ============================================================
        // 1. MASTER DATABASE
        // ============================================================

        public IDbConnection CreateMasterConnection()
        {
            var masterconnectionstring = _configuration.GetConnectionString("MasterDB");

            if (string.IsNullOrWhiteSpace(masterconnectionstring))
                throw new InvalidOperationException($"Connection string '{masterconnectionstring}' not found.");

            return new SqlConnection(masterconnectionstring);
        }

        // ============================================================
        // 2. NORMAL CLIENT DATABASE CONNECTION
        // BranchCode comes from JWT
        // ============================================================

        public async Task<IDbConnection> CreateConnection( string databaseType)
        {
            var branchCode = _tenantContext.BranchCode;

            if (string.IsNullOrWhiteSpace(branchCode))
            {
                throw new UnauthorizedAccessException( "BranchCode is not available in JWT.");
            }

            return await CreateClientConnection( branchCode, databaseType);
        }

        // ============================================================
        // 3. LOGIN CLIENT DATABASE CONNECTION
        // BranchCode comes from Master DB result
        // ============================================================

        public async Task<IDbConnection> CreateConnection( string branchCode, string databaseType)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
            {
                throw new ArgumentException( "BranchCode is required.", nameof(branchCode));
            }

            return await CreateClientConnection( branchCode, databaseType);
        }

        // ============================================================
        // 4. COMMON CLIENT CONNECTION LOGIC
        // ============================================================

        private async Task<IDbConnection> CreateClientConnection( string branchCode, string databaseType)
        {
            var cacheKey = $"TenantDb:{branchCode}:{databaseType}";

            // --------------------------------------------------------
            // Check Cache
            // --------------------------------------------------------

            if (_cache.TryGetValue( cacheKey, out string? connectionString))
            {
                return new SqlConnection(connectionString);
            }

            // --------------------------------------------------------
            // Cache Miss -> Master DB
            // --------------------------------------------------------

            connectionString = await GetConnectionStringFromMaster( branchCode, databaseType);

            // --------------------------------------------------------
            // Store in Cache
            // --------------------------------------------------------

            var cacheOptions =
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(30),

                    SlidingExpiration =
                        TimeSpan.FromMinutes(10)
                };

            _cache.Set( cacheKey, connectionString, cacheOptions);

            // --------------------------------------------------------
            // Return Client DB Connection
            // --------------------------------------------------------

            return new SqlConnection(connectionString);
        }

        // ============================================================
        // 5. GET CLIENT DATABASE CONFIGURATION FROM MASTER DB
        // ============================================================

        private async Task<string> GetConnectionStringFromMaster(
            string branchCode,
            string databaseType)
        {
            var masterConnectionString =
                _configuration.GetConnectionString("MasterDB");

            if (string.IsNullOrWhiteSpace(
                masterConnectionString))
            {
                throw new InvalidOperationException(
                    "MasterDB connection string not found.");
            }

            await using var master =
                new SqlConnection(masterConnectionString);

            const string sql = @"
                SELECT
                    ServerName,
                    DatabaseName,
                    UserName,
                    Password
                FROM MultiTenantDatabases
                WHERE BranchCode = @BranchCode
                  AND DatabaseType = @DatabaseType;";


            var db =
                await master.QueryFirstOrDefaultAsync
                    <ClientDatabasemodel>(
                        sql,
                        new
                        {
                            BranchCode = branchCode,
                            DatabaseType = databaseType
                        });


            if (db == null)
            {
                throw new InvalidOperationException(
                    $"Database configuration not found. " +
                    $"BranchCode: {branchCode}, " +
                    $"DatabaseType: {databaseType}");
            }


            var builder =
                new SqlConnectionStringBuilder
                {
                    DataSource = db.ServerName,
                    InitialCatalog = db.DatabaseName,
                    UserID = db.UserName,
                    Password = db.Password,
                    ConnectTimeout = 20,
                    TrustServerCertificate = true
                };


            return builder.ConnectionString;
        }

        // ============================================================
        // 6. CLEAR CACHE
        // ============================================================

        public void ClearConnectionCache( string branchCode, string databaseType)
        {
            var cacheKey = $"TenantDb:{branchCode}:{databaseType}";

            _cache.Remove(cacheKey);
        }

        //public async Task<IDbConnection> CreateConnection(string databaseType)
        //{
        //    var branchCode = _tenantContext.BranchCode;

        //    if (string.IsNullOrWhiteSpace(branchCode))
        //    {
        //        throw new UnauthorizedAccessException(
        //            "BranchCode is not available.");
        //    }

        //    // Example:
        //    // TenantDb:BR001:POS
        //    var cacheKey = $"TenantDb:{branchCode}:{databaseType}";

        //    // ----------------------------------------
        //    // 1. Check Cache
        //    // ----------------------------------------

        //    if (_cache.TryGetValue(
        //        cacheKey,
        //        out string? connectionString))
        //    {
        //        return new SqlConnection(connectionString);
        //    }

        //    // ----------------------------------------
        //    // 2. Cache Miss Go to Master DB
        //    // ----------------------------------------

        //    connectionString = await GetConnectionStringFromMaster( branchCode, databaseType);


        //    // ----------------------------------------
        //    // 3. Store in Cache
        //    // ----------------------------------------

        //    var cacheOptions =
        //        new MemoryCacheEntryOptions
        //        {
        //            AbsoluteExpirationRelativeToNow =
        //                TimeSpan.FromMinutes(30),

        //            SlidingExpiration =
        //                TimeSpan.FromMinutes(10)
        //        };

        //    _cache.Set( cacheKey, connectionString, cacheOptions);

        //    // ----------------------------------------
        //    // 4. Return Client DB Connection
        //    // ----------------------------------------

        //    return new SqlConnection(connectionString);
        //}

        //public async Task<string> GetConnectionStringFromMaster( string branchCode, string databaseType)
        //{
        //    var masterConnectionString = _configuration.GetConnectionString("MasterDB");

        //    if (string.IsNullOrWhiteSpace(masterConnectionString))
        //    {
        //        throw new InvalidOperationException(
        //            "MasterDB connection string not found.");
        //    }

        //    await using var master = new SqlConnection(masterConnectionString);

        //    const string sql = @"
        //        SELECT
        //            ServerName,
        //            DatabaseName,
        //            UserName,
        //            Password
        //        FROM MultiTenantDatabases
        //        WHERE BranchCode = @BranchCode
        //          AND DatabaseType = @DatabaseType;";

        //    var db = await master.QueryFirstOrDefaultAsync<ClientDatabasemodel>( sql, new { BranchCode = branchCode, DatabaseType = databaseType });

        //    if (db == null)
        //    {
        //        throw new InvalidOperationException(
        //            $"Database configuration not found. " +
        //            $"BranchCode: {branchCode}, " +
        //            $"DatabaseType: {databaseType}");
        //    }

        //    var builder = new SqlConnectionStringBuilder
        //    {
        //        DataSource = db.ServerName,
        //        InitialCatalog = db.DatabaseName,
        //        UserID = db.UserName,
        //        Password = db.Password,
        //        ConnectTimeout = 20000,
        //        TrustServerCertificate = true
        //    };

        //    return builder.ConnectionString;
        //}

        //public void ClearConnectionCache( string branchCode, string databaseType)
        //{
        //    var cacheKey = $"TenantDb:{branchCode}:{databaseType}";

        //    _cache.Remove(cacheKey);
        //}

        //public async Task<IDbConnection> CreateConnection(string BranchCode,    string DatabaseType)
        //{
        //    using var master = new SqlConnection(_configuration.GetConnectionString("MasterDB"));

        //    const string sql = "SELECT ServerName, DatabaseName, UserName, Password FROM MultiTenantDatabases WHERE BranchCode = @branchcode AND DatabaseType = @databaseType";

        //    var db = await master.QueryFirstOrDefaultAsync<ClientDatabasemodel>(sql, new { branchcode = BranchCode, databaseType = DatabaseType });

        //    if (db == null)
        //        throw new Exception("Database configuration not found.");

        //    var builder = new SqlConnectionStringBuilder
        //    {
        //        DataSource = db.ServerName,
        //        InitialCatalog = db.DatabaseName,
        //        UserID = db.UserName,
        //        Password = db.Password,
        //        ConnectTimeout = 20000,
        //        TrustServerCertificate = true
        //    };

        //    return new SqlConnection(builder.ConnectionString);

        //    //var connectionString =
        //    //    $" Source = {db.ServerName}; " +
        //    //    $" Database = {db.DatabaseName}; " +
        //    //    $" User Id = {db.UserName}; " +
        //    //    $" Password = {db.Password}; " +
        //    //    $" Connection Timeout = 20000; " +
        //    //    $" TrustServerCertificate = True; ";

        //    //return new SqlConnection(connectionString);
        //}
    }
}
