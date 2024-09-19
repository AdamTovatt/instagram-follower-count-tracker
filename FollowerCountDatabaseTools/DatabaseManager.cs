using Dapper;
using DbUp;
using DbUp.Engine;
using FollowerCountDatabaseTools.Models;
using Npgsql;
using System.Data.Common;
using System.Reflection;
using System.Text;

namespace FollowerCountDatabaseTools
{
    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = GetConnectionStringFromUrl(connectionString, SslMode.Prefer);
        }

        private static string GetConnectionStringFromUrl(string url, SslMode sslMode = SslMode.Require, bool trustServerCertificate = true)
        {
            try
            {
                Uri uri = new Uri(url);
                string[] array = uri.UserInfo.Split(':');
                ConnectionStringBuilder connectionStringBuilder = new ConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port,
                    Username = array[0],
                    Password = array[1],
                    Database = uri.LocalPath.TrimStart('/'),
                    SslMode = sslMode,
                    TrustServerCertificate = trustServerCertificate
                };
                return connectionStringBuilder.ToString();
            }
            catch
            {
                throw new Exception($"Unknown error when creating connection string. Connection string should be in the following format: postgres://username:password@hostname:port/database \nThe connection string was: {url}");
            }
        }

        public void SetupDatabase()
        {
            PerformMigrations(); // perform database migrations
            DefaultTypeMap.MatchNamesWithUnderscores = true; // set up dapper to match column names with underscore
        }

        private void PerformMigrations()
        {
            UpgradeEngine upgrader =
                DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), (string s) => { return s.Contains("DatabaseMigrations") && s.Split(".").Last() == "sql"; })
                    .LogToConsole()
                    .Build();

            DatabaseUpgradeResult result = upgrader.PerformUpgrade();

            if (!result.Successful)
                throw new Exception($"Error when performing database upgrade, failing on script: {result.ErrorScript?.Name} with error {result.Error}");
        }

        protected async Task<NpgsqlConnection> GetConnectionAsync()
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task StoreAccountInfoAsync(params AccountInfo[] accountInfos)
        {
            if (accountInfos == null || accountInfos.Length == 0)
            {
                throw new ArgumentException("At least one account info must be provided.", nameof(accountInfos));
            }

            const string query = @"
        INSERT INTO account_info (name, followers, following, posts)
        VALUES (@Name, @Followers, @Following, @Posts)";

            using (NpgsqlConnection connection = await GetConnectionAsync())
            {
                using (DbTransaction transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (AccountInfo accountInfo in accountInfos)
                        {
                            await connection.ExecuteAsync(query, new
                            {
                                Name = accountInfo.Name,
                                Followers = accountInfo.Followers,
                                Following = accountInfo.Following,
                                Posts = accountInfo.Posts
                            }, (NpgsqlTransaction)transaction); // Explicit cast to NpgsqlTransaction
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        private class ConnectionStringBuilder
        {
            public string? Host { get; set; }

            public int Port { get; set; }

            public string? Username { get; set; }

            public string? Password { get; set; }

            public string? Database { get; set; }

            public SslMode SslMode { get; set; }

            public bool TrustServerCertificate { get; set; }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (Host != null)
                {
                    stringBuilder.Append("Host=");
                    stringBuilder.Append(Host);
                    stringBuilder.Append(";");
                }

                if (Port != 0)
                {
                    stringBuilder.Append("Port=");
                    stringBuilder.Append(Port);
                    stringBuilder.Append(";");
                }

                if (Username != null)
                {
                    stringBuilder.Append("Username=");
                    stringBuilder.Append(Username);
                    stringBuilder.Append(";");
                }

                if (Password != null)
                {
                    stringBuilder.Append("Password=");
                    stringBuilder.Append(Password);
                    stringBuilder.Append(";");
                }

                if (Database != null)
                {
                    stringBuilder.Append("Database=");
                    stringBuilder.Append(Database);
                    stringBuilder.Append(";");
                }

                stringBuilder.Append("SSL Mode=");
                stringBuilder.Append(SslMode.ToString());
                stringBuilder.Append(";");
                stringBuilder.Append("Trust Server Certificate=");
                stringBuilder.Append(TrustServerCertificate);
                return stringBuilder.ToString();
            }
        }
    }
}
