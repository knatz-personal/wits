// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  © 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using System.Reflection;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using WITS.Migrator.Migrations;

namespace WITS.Migrator
{
    /// <summary>
    /// Main program class for WITS.Migrator.
    /// Handles command-line operations for database migrations.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point of the application.
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Load configuration from various sources
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build();

                // Get database connection string from configuration
                var connectionString = config.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("DefaultConnection not found in configuration");

                // Ensure database directory exists
                var dbPath = Path.GetDirectoryName(connectionString.Split('=')[1]);
                if (!string.IsNullOrEmpty(dbPath) && !Directory.Exists(dbPath))
                {
                    Directory.CreateDirectory(dbPath);
                }

                // Parse command-line arguments
                if (args.Length == 0)
                {
                    ShowHelp();
                    return 0;
                }

                var command = args[0].ToLower();
                var parameters = args.Skip(1).ToArray();

                // Execute the appropriate command
                return command switch
                {
                    "migrate" => await Migrate(connectionString),
                    "rollback" => await Rollback(connectionString, parameters),
                    "list" => await List(connectionString),
                    "list-migrations" => await ListMigrations(connectionString),
                    "list-tables" => await ListTables(connectionString),
                    "remove" => await Remove(connectionString, parameters),
                    "create" => await Create(parameters),
                    _ => ShowHelp()
                };
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return 1;
            }
        }

        /// <summary>
        /// Displays help information for available commands.
        /// </summary>
        /// <returns>Always returns 1 to indicate help was shown</returns>
        private static int ShowHelp()
        {
            var table = new Table();
            table.AddColumn("Command");
            table.AddColumn("Description");
            table.AddColumn("Usage");

            table.AddRow("migrate", "Apply all pending migrations", "migrate");
            table.AddRow("rollback", "Rollback a specific migration", "rollback <migration_id>");
            table.AddRow("list", "List all migrations and tables", "list");
            table.AddRow("list-migrations", "List all migrations", "list-migrations");
            table.AddRow("list-tables", "List all database tables", "list-tables");
            table.AddRow("remove", "Remove a migration", "remove <migration_id|all>");
            table.AddRow("create", "Create a new migration", "create <migration_name>");

            AnsiConsole.Write(table);
            return 1;
        }

        /// <summary>
        /// Applies all pending migrations to the database.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        private static async Task<int> Migrate(string connectionString)
        {
            var migrations = GetMigrations();
            if (!migrations.Any())
            {
                AnsiConsole.MarkupLine("[yellow]No migrations found to apply.[/]");
                return 0;
            }

            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("[blue]Migration ID[/]").Centered());
            table.AddColumn(new TableColumn("[blue]Name[/]").Centered());
            table.AddColumn(new TableColumn("[blue]Status[/]").Centered());

            foreach (var migration in migrations)
            {
                try
                {
                    AnsiConsole.MarkupLine($"Applying migration: [blue]{migration.Name}[/]");
                    migration.Apply();
                    table.AddRow(migration.MigrationId, migration.Name, "[green]Applied[/]");
                }
                catch (Exception ex)
                {
                    table.AddRow(migration.MigrationId, migration.Name, "[red]Failed[/]");
                    AnsiConsole.MarkupLine($"[red]Error applying migration {migration.Name}: {ex.Message}[/]");
                    throw;
                }
            }

            AnsiConsole.Write(table);
            return 0;
        }

        /// <summary>
        /// Rolls back a specific migration.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="parameters">Command parameters containing migration ID</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        private static async Task<int> Rollback(string connectionString, string[] parameters)
        {
            if (parameters.Length < 1)
            {
                AnsiConsole.MarkupLine("[red]Please specify migration ID to rollback[/]");
                return 1;
            }

            var migrationId = parameters[0];
            var migrations = GetMigrations();
            var migration = migrations.FirstOrDefault(m => m.MigrationId == migrationId);

            if (migration == null)
            {
                AnsiConsole.MarkupLine($"[red]Migration {migrationId} not found[/]");
                return 1;
            }

            try
            {
                AnsiConsole.MarkupLine($"Rolling back migration: [blue]{migration.Name}[/]");
                migration.Rollback();
                AnsiConsole.MarkupLine("[green]Migration rolled back successfully[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error rolling back migration {migration.Name}: {ex.Message}[/]");
                throw;
            }

            return 0;
        }

        /// <summary>
        /// Lists all migrations and database tables.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        private static async Task<int> List(string connectionString)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'Migrations';";
                var reader = command.ExecuteReader();

                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn(new TableColumn("[blue]Table Name[/]").Centered());

                var hasRows = false;
                while (reader.Read())
                {
                    hasRows = true;
                    table.AddRow(reader.GetString(0));
                }

                if (!hasRows)
                {
                    AnsiConsole.MarkupLine("[yellow]No tables found in the database.[/]");
                    return 0;
                }

                AnsiConsole.Write(table);
            }

            return 0;
        }

        /// <summary>
        /// Lists all migrations in the database.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        private static async Task<int> ListMigrations(string connectionString)
        {
            var appliedMigrations = GetAppliedMigrations();
            var migrations = GetMigrations().OrderBy(m => m.MigrationId);

            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn(new TableColumn("[blue]Migration ID[/]").Centered());
            table.AddColumn(new TableColumn("[blue]Name[/]").Centered());
            table.AddColumn(new TableColumn("[blue]Status[/]").Centered());

            foreach (var migration in migrations)
            {
                var status = appliedMigrations.Contains(migration.MigrationId) ? "[green]Applied[/]" : "[yellow]Pending[/]";
                table.AddRow(migration.MigrationId, migration.Name, status);
            }

            AnsiConsole.Write(table);
            return 0;
        }

        /// <summary>
        /// Lists all tables in the database.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        private static async Task<int> ListTables(string connectionString)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'Migrations';";
                var reader = command.ExecuteReader();

                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn(new TableColumn("[blue]Table Name[/]").Centered());

                var hasRows = false;
                while (reader.Read())
                {
                    hasRows = true;
                    table.AddRow(reader.GetString(0));
                }

                if (!hasRows)
                {
                    AnsiConsole.MarkupLine("[yellow]No tables found in the database.[/]");
                    return 0;
                }

                AnsiConsole.Write(table);
            }

            return 0;
        }

        /// <summary>
        /// Removes a specific migration or all migrations.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="parameters">Command parameters containing migration ID or 'all'</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        private static async Task<int> Remove(string connectionString, string[] parameters)
        {
            var migrations = GetMigrations().ToList();
            var appliedMigrations = GetAppliedMigrations();

            if (parameters.Length < 1)
            {
                AnsiConsole.MarkupLine("[red]Please specify migration ID to remove, or use 'all' to remove all pending migrations[/]");
                return 1;
            }

            var migrationId = parameters[0];

            if (migrationId.ToLower() == "all")
            {
                var pendingMigrations = migrations.Where(m => !appliedMigrations.Contains(m.MigrationId)).ToList();
                if (!pendingMigrations.Any())
                {
                    AnsiConsole.MarkupLine("[yellow]No pending migrations to remove.[/]");
                    return 0;
                }

                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.AddColumn(new TableColumn("[blue]Migration ID[/]").Centered());
                table.AddColumn(new TableColumn("[blue]Name[/]").Centered());
                table.AddColumn(new TableColumn("[blue]Status[/]").Centered());

                foreach (var migration in pendingMigrations)
                {
                    RemoveMigrationFile(migration.MigrationId);
                    table.AddRow(migration.MigrationId, migration.Name, "[red]Removed[/]");
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[green]Removed {pendingMigrations.Count} pending migration(s).[/]");
            }
            else
            {
                var migration = migrations.FirstOrDefault(m => m.MigrationId == migrationId);
                if (migration == null)
                {
                    AnsiConsole.MarkupLine($"[red]Migration {migrationId} not found[/]");
                    return 1;
                }

                if (appliedMigrations.Contains(migrationId))
                {
                    AnsiConsole.MarkupLine($"[red]Cannot remove migration {migrationId} because it has been applied. Rollback first.[/]");
                    return 1;
                }

                RemoveMigrationFile(migrationId);
                AnsiConsole.MarkupLine($"[green]Removed migration {migrationId}[/]");
            }

            return 0;
        }

        /// <summary>
        /// Creates a new migration file.
        /// </summary>
        /// <param name="parameters">Command parameters containing migration name</param>
        /// <returns>Exit code (0 for success, non-zero for failure)</returns>
        private static async Task<int> Create(string[] parameters)
        {
            if (parameters.Length < 1)
            {
                AnsiConsole.MarkupLine("[red]Please specify migration name[/]");
                return 1;
            }

            var name = parameters[0];
            // Generate timestamp in format YYYYMMDDHHMMSS
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var migrationId = timestamp;
            var className = $"Migration_{timestamp}_{name}";
            var fileName = $"{timestamp}_{name}.cs";
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Migrations");

            // Ensure Migrations directory exists
            if (!Directory.Exists(migrationsPath))
            {
                Directory.CreateDirectory(migrationsPath);
            }

            var filePath = Path.Combine(migrationsPath, fileName);

            // Check if file already exists
            if (File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]Migration file {fileName} already exists[/]");
                return 0;
            }

            // Create migration file content
            var content = $@"using Microsoft.Data.Sqlite;

namespace WITS.Migrator.Migrations
{{
    public class {className} : Migration
    {{
        public override string MigrationId => ""{migrationId}"";
        public override string Name => ""{name}"";

        public override string UpScript => @""
            -- TODO: Add your migration SQL here
            -- Example: CREATE TABLE IF NOT EXISTS TableName (
            --     Id INTEGER PRIMARY KEY AUTOINCREMENT,
            --     CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
            -- )"";

        public override string DownScript => @""
            -- TODO: Add your rollback SQL here
            -- Example: DROP TABLE IF EXISTS TableName"";
    }}
}}";

            // Write the file
            File.WriteAllText(filePath, content);
            AnsiConsole.MarkupLine($"[green]Created migration file: {fileName}[/]");
            AnsiConsole.MarkupLine("[yellow]Please edit the UpScript and DownScript with your migration SQL[/]");

            return 0;
        }

        /// <summary>
        /// Gets a list of all migration files in the Migrations directory.
        /// </summary>
        /// <returns>List of migration file paths</returns>
        private static List<string> GetMigrationFiles()
        {
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Migrations");
            return Directory.GetFiles(migrationsPath, "*.cs").ToList();
        }

        /// <summary>
        /// Gets the status of migrations in the database.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <returns>Dictionary of migration IDs and their status</returns>
        private static async Task<Dictionary<string, string>> GetMigrationStatus(SqliteConnection connection)
        {
            var appliedMigrations = new Dictionary<string, string>();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT MigrationId FROM __Migrations;";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                appliedMigrations.Add(reader.GetString(0), "Applied");
            }
            return appliedMigrations;
        }

        /// <summary>
        /// Gets a list of all tables in the database.
        /// </summary>
        /// <param name="connection">Database connection</param>
        /// <returns>List of table names</returns>
        private static async Task<List<string>> GetTables(SqliteConnection connection)
        {
            var tables = new List<string>();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name NOT LIKE 'Migrations';";
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tables.Add(reader.GetString(0));
            }
            return tables;
        }

        /// <summary>
        /// Ensures the migrations table exists in the database.
        /// </summary>
        /// <param name="connection">Database connection</param>
        private static async Task EnsureMigrationsTable(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE IF NOT EXISTS __Migrations (MigrationId TEXT PRIMARY KEY);";
            await command.ExecuteNonQueryAsync();
        }

        static void RemoveMigrationFile(string migrationId)
        {
            var migrationsPath = Path.Combine(Directory.GetCurrentDirectory(), "Migrations");
            var files = Directory.GetFiles(migrationsPath, $"{migrationId}_*.cs");

            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                    AnsiConsole.MarkupLine($"Deleted file: [blue]{Path.GetFileName(file)}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error deleting file {file}: {ex.Message}[/]");
                }
            }
        }

        static HashSet<string> GetAppliedMigrations()
        {
            var appliedMigrations = new HashSet<string>();
            var connectionString = GetConnectionString();
            var builder = new SqliteConnectionStringBuilder(connectionString);

            if (!File.Exists(builder.DataSource))
            {
                return appliedMigrations;
            }

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            try
            {
                using var cmd = new SqliteCommand("SELECT MigrationId FROM __Migrations", connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    appliedMigrations.Add(reader.GetString(0));
                }
            }
            catch (SqliteException)
            {
                // If __Migrations table doesn't exist, no migrations have been applied
            }

            return appliedMigrations;
        }

        static IEnumerable<Migration> GetMigrations()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var migrationTypes = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Migration)) && !t.IsAbstract)
                .OrderBy(t => t.Name); // Names now contain timestamps, so this sorts chronologically

            return migrationTypes.Select(t => (Migration)Activator.CreateInstance(t));
        }

        static string GetConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return config.GetConnectionString("DefaultConnection");
        }
    }
}
