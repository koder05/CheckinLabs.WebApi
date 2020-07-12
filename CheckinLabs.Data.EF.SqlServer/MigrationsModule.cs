using CheckinLabs.AppBase;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CheckinLabs.Data.EF.SqlServer
{
    [ModuleDependency(typeof(DbModule))]
    public sealed class MigrationsModule : StartModuleBase
    {
        private static readonly IReadOnlyList<Func<SqlConnection, int, CancellationToken, Task<int>>> MigrationsList;
        private string cs;
        static MigrationsModule()
        {
            MigrationsList = new List<Func<SqlConnection, int, CancellationToken, Task<int>>>
            {
                (c,ver, t) => MigrActionAsync("CREATE TABLE dbo.MigrationsVersions(id int IDENTITY NOT NULL,Version int NOT NULL,Migrated datetime2(0) NOT NULL,PRIMARY KEY CLUSTERED (id),UNIQUE (Version))"
                        ,c, ver, t)
                , MigrationScriptFromResource("AccountTables")
            };
        }


        private ILogger<MigrationsModule> logger;
        public MigrationsModule(IConfiguration cfg, ILogger<MigrationsModule> logger)
        {
            this.logger = logger;
            cs = cfg.GetConnectionString(AppDbContext.DbConnectionStringKey);
        }

        private async Task<int> GetDbVersion(SqlConnection conn, CancellationToken cancellationToken)
        {
            using (var cmd = new SqlCommand(string.Empty, conn))
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                    await cmd.Connection.OpenAsync(cancellationToken);
                cmd.CommandText = "SELECT CAST(CASE WHEN OBJECT_ID (N'MigrationsVersions', N'U') IS NULL THEN 0 ELSE 1 END as bit)";
                var ret = await cmd.ExecuteScalarAsync(cancellationToken);
                if ((bool)ret)
                {
                    cmd.CommandText = "SELECT MAX(Version) FROM MigrationsVersions";
                    return (int)await cmd.ExecuteScalarAsync(cancellationToken);
                }
                return -1;
            }
        }

        protected override async Task RunInnerAsync(CancellationToken cancellationToken)
        {
            using (SqlConnection conn = new SqlConnection(cs))
            {
                int dbVersion = await GetDbVersion(conn, cancellationToken);
                int codeVersion = MigrationsList.Count - 1;

                if (codeVersion == dbVersion)
                {
                    logger.LogInformation($"Compatible [{codeVersion}].");
                    return;
                }

                if (dbVersion > codeVersion)
                {
                    throw new Exception($"The DB version[{ dbVersion }] is higher than the current code's version [{codeVersion}]. Application cannot run.");
                }

                logger.LogInformation($"The DB version [{dbVersion}] is lower than the code version [{codeVersion}]. Migration process is begun.");

                do
                {
                    int newDbVersion = dbVersion + 1;
                    logger.LogInformation($"Starting migration from {dbVersion} to {newDbVersion}.");
                    var migrAction = MigrationsList[newDbVersion];
                    try
                    {
                        int changes = await migrAction(conn, newDbVersion, cancellationToken).ConfigureAwait(false);
                        logger.LogInformation($"Successfully migrated from {dbVersion} to {newDbVersion} with {changes} changes.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to migrate from {dbVersion} to {newDbVersion}.", ex);
                        throw;
                    }
                    dbVersion = newDbVersion;
                } while (dbVersion < codeVersion);
            }

        }

        private static async Task<int> MigrActionAsync(string sql, SqlConnection conn, int ver, CancellationToken cancellationToken)
        {
            int result = -1;
            using (var cmd = new SqlCommand(string.Empty, conn))
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                    await cmd.Connection.OpenAsync(cancellationToken);
                using (var scope = cmd.Connection.BeginTransaction())
                {
                    cmd.Transaction = scope;
                    string sqlBatch = string.Empty;
                    sql += "\nGO";
                    foreach (string line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.ToUpperInvariant().Trim() == "GO" && !string.IsNullOrEmpty(sqlBatch))
                        {
                            cmd.CommandText = sqlBatch;
                            result = await cmd.ExecuteNonQueryAsync(cancellationToken);
                            sqlBatch = string.Empty;
                        }
                        else
                        {
                            sqlBatch += line + "\n";
                        }
                    }
                    cmd.CommandText = "insert MigrationsVersions(Version,Migrated)values(@Version,@Date)";
                    cmd.Parameters.AddWithValue("@Version", ver);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                    scope.Commit();
                }
            }
            return result;
        }
        private static Func<SqlConnection, int, CancellationToken, Task<int>> MigrationScriptFromResource(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith($".{fileName}.sql"));
            if (!string.IsNullOrEmpty(resourceName))
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(stream))
                {
                    var cmd = reader.ReadToEnd();
                    return new Func<SqlConnection, int, CancellationToken, Task<int>>((c, ver, t) => MigrActionAsync(cmd, c, ver, t));
                }
            }
            return new Func<SqlConnection, int, CancellationToken, Task<int>>((c, ver, t) => throw new Exception($"Unknown resource file '{fileName}.sql'"));
        }
    }
}
