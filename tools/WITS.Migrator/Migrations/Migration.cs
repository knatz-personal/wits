// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using System;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace WITS.Migrator.Migrations
{
    public abstract class Migration
    {
        public abstract string MigrationId { get; }
        public abstract string Name { get; }
        public abstract string UpScript { get; }
        public abstract string DownScript { get; }

        protected string GetConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return config.GetConnectionString("DefaultConnection");
        }

        protected void EnsureDatabaseExists()
        {
            var connectionString = GetConnectionString();
            var builder = new SqliteConnectionStringBuilder(connectionString);
            var dataSource = builder.DataSource;
            var dataDirectory = Path.GetDirectoryName(dataSource);

            if (!string.IsNullOrEmpty(dataDirectory) && !Directory.Exists(dataDirectory))
            {
                Console.WriteLine($"Creating data directory at {dataDirectory}");
                Directory.CreateDirectory(dataDirectory);
            }

            if (!File.Exists(dataSource))
            {
                Console.WriteLine($"Database file not found at {dataSource}. Creating new database...");
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                // Create migrations table if it doesn't exist
                using var cmd = new SqliteCommand(@"
                    CREATE TABLE IF NOT EXISTS __Migrations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        MigrationId TEXT NOT NULL UNIQUE,
                        Name TEXT NOT NULL,
                        AppliedAt DATETIME NOT NULL,
                        Script TEXT NOT NULL
                    );

                    CREATE INDEX IF NOT EXISTS idx_migrations_id ON __Migrations(MigrationId);", connection);
                cmd.ExecuteNonQuery();

                Console.WriteLine("Database and migrations table created successfully.");
            }
        }

        public void Apply()
        {
            EnsureDatabaseExists();

            using var connection = new SqliteConnection(GetConnectionString());
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Check if migration already applied
                var checkSql = "SELECT COUNT(*) FROM __Migrations WHERE MigrationId = @MigrationId";
                using var checkCmd = new SqliteCommand(checkSql, connection, transaction);
                checkCmd.Parameters.AddWithValue("@MigrationId", MigrationId);
                var exists = (long)checkCmd.ExecuteScalar() > 0;

                if (!exists)
                {
                    // Apply migration
                    using var cmd = new SqliteCommand(UpScript, connection, transaction);
                    cmd.ExecuteNonQuery();

                    // Record migration
                    var insertSql = @"
                        INSERT INTO __Migrations (MigrationId, Name, AppliedAt, Script)
                        VALUES (@MigrationId, @Name, @AppliedAt, @Script)";
                    
                    using var insertCmd = new SqliteCommand(insertSql, connection, transaction);
                    insertCmd.Parameters.AddWithValue("@MigrationId", MigrationId);
                    insertCmd.Parameters.AddWithValue("@Name", Name);
                    insertCmd.Parameters.AddWithValue("@AppliedAt", DateTime.UtcNow);
                    insertCmd.Parameters.AddWithValue("@Script", UpScript);
                    insertCmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Rollback()
        {
            EnsureDatabaseExists();

            using var connection = new SqliteConnection(GetConnectionString());
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Apply rollback script
                using var cmd = new SqliteCommand(DownScript, connection, transaction);
                cmd.ExecuteNonQuery();

                // Remove migration record
                var deleteSql = "DELETE FROM __Migrations WHERE MigrationId = @MigrationId";
                using var deleteCmd = new SqliteCommand(deleteSql, connection, transaction);
                deleteCmd.Parameters.AddWithValue("@MigrationId", MigrationId);
                deleteCmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
} 
