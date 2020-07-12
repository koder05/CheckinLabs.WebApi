using CheckinLabs.AppBase;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CheckinLabs.Data.EF.SqlServer
{
    public sealed class DbModule : StartModuleBase
    {
        private ILogger<DbModule> logger;
        private string cs;
        public DbModule(IConfiguration cfg, ILogger<DbModule> logger)
        {
            this.logger = logger;
            cs = cfg.GetConnectionString(AppDbContext.DbConnectionStringKey);
        }
        protected override async Task RunInnerAsync(CancellationToken cancellationToken)
        {
            try
            {
                var connStrBuilder = new SqlConnectionStringBuilder(cs);
                var dbName = connStrBuilder.InitialCatalog;
                connStrBuilder.InitialCatalog = "master";
                using (var connection = new SqlConnection(connStrBuilder.ConnectionString))
                {
                    await connection.OpenAsync(cancellationToken);
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"IF NOT EXISTS(select * from sys.databases where name='{dbName}') CREATE DATABASE {dbName}";
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    connection.Close();
                    connection.Dispose();
                }
                logger.LogInformation("OK");
            }
            catch (Exception ex)
            {
                logger.LogError("No connection.", ex);
                throw;
            }
        }
    }
}
