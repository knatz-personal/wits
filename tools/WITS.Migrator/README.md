# WITS Database Migration Tool

A command-line tool for managing database migrations in the WITS project. This tool provides a simple and efficient way to apply, rollback, and manage database migrations.

## Features

- Apply pending migrations
- Rollback specific migrations
- List all migrations and their status
- List all database tables
- Remove pending migrations
- Beautiful console UI with Spectre.Console
- Transaction support for safe migrations
- SQLite database support
- Convenient batch files for common operations

## Prerequisites

- .NET 9.0 SDK or later
- SQLite database

## Configuration

The tool uses an `appsettings.json` file for configuration. Here's an example configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=path/to/your/database.db"
  }
}
```

## Quick Start - Batch Files

The tool comes with convenient batch files for common operations:

1. `migrate.bat` - Apply all pending migrations
2. `rollback.bat <migration_id>` - Rollback a specific migration
3. `list-migrations.bat` - Show all migrations and their status
4. `list-tables.bat` - Show all tables in the database
5. `remove.bat <migration_id|all>` - Remove pending migration(s)
6. `create.bat <migration_name>` - Create a new migration

Example usage:
```batch
migrate.bat
list-migrations.bat
list-tables.bat
create.bat AddUserTable
rollback.bat 20240321000000
remove.bat all
```

## Detailed Commands

### 1. List Migrations

Shows all available migrations and their current status.

Using batch file:
```batch
list-migrations.bat
```

Using dotnet command:
```bash
dotnet run -- list-migrations
```

Example output:
```
╭────────────────┬───────────────┬─────────╮
│  Migration ID  │     Name      │ Status  │
├────────────────┼───────────────┼─────────┤
│ 20240321000000 │ TestMigration │ Applied │
╰────────────────┴───────────────┴─────────╯
```

### 2. List Tables

Shows all tables in the database.

Using batch file:
```batch
list-tables.bat
```

Using dotnet command:
```bash
dotnet run -- list-tables
```

Example output:
```
╭─────────────────────╮
│     Table Name      │
├─────────────────────┤
│ Users               │
│ Projects           │
│ Tasks              │
╰─────────────────────╯
```

### 3. Apply Migrations

Applies all pending migrations in chronological order.

Using batch file:
```batch
migrate.bat
```

Using dotnet command:
```bash
dotnet run -- migrate
```

Example output:
```
Applying migration: TestMigration
╭────────────────┬───────────────┬─────────╮
│  Migration ID  │     Name      │ Status  │
├────────────────┼───────────────┼─────────┤
│ 20240321000000 │ TestMigration │ Applied │
╰────────────────┴───────────────┴─────────╯
```

### 4. Rollback Migration

Rolls back a specific migration by its ID.

Using batch file:
```batch
rollback.bat 20240321000000
```

Using dotnet command:
```bash
dotnet run -- rollback 20240321000000
```

Example output:
```
Rolling back migration: TestMigration
Migration rolled back successfully
```

### 5. Remove Migration

Removes a pending migration file. This command can be used to remove either a specific migration or all pending migrations.

Using batch file:
To remove a specific migration:
```batch
remove.bat 20240321000000
```

To remove all pending migrations:
```batch
remove.bat all
```

Using dotnet command:
```bash
dotnet run -- remove 20240321000000
# or
dotnet run -- remove all
```

Example output:
```
Deleted file: 20240321000000_TestMigration.cs
Removed migration 20240321000000
```

### 6. Create Migration

Creates a new migration file with the specified name.

Using batch file:
```batch
create.bat AddUserTable
```

Using dotnet command:
```bash
dotnet run -- create AddUserTable
```

Example output:
```
Created migration file: 20240321123456_AddUserTable.cs
Please edit the UpScript and DownScript with your migration SQL
```

## Creating Migrations

To create a new migration, create a new class in the `Migrations` folder that inherits from the `Migration` base class. The class name should follow the pattern `{Timestamp}_{MigrationName}.cs`.

Example migration:
```csharp
using Microsoft.Data.Sqlite;

namespace WITS.DbManager.Migrations
{
    public class TestMigration : Migration
    {
        public override string MigrationId => "20240321000000";
        public override string Name => "TestMigration";

        public override string UpScript => @"
            CREATE TABLE TestTable (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL
            )";

        public override string DownScript => "DROP TABLE IF EXISTS TestTable";
    }
}
```

## Migration Properties

- `MigrationId`: A unique identifier for the migration (typically a timestamp)
- `Name`: A descriptive name for the migration
- `UpScript`: The SQL script to apply the migration
- `DownScript`: The SQL script to rollback the migration

## Safety Features

1. **Transaction Support**: All migrations are wrapped in transactions to ensure database consistency
2. **Applied Migration Protection**: Cannot remove migrations that have been applied
3. **Rollback Safety**: Each migration must provide a rollback script
4. **Status Tracking**: The tool maintains a record of all applied migrations
5. **Error Handling**: Detailed error messages and proper cleanup on failure

## Best Practices

1. Always test migrations in a development environment first
2. Keep migration scripts idempotent when possible
3. Include rollback scripts for all migrations
4. Use descriptive names for migrations
5. Follow the timestamp naming convention for migration files
6. Commit migration files to version control
7. Never modify migrations that have been applied to production

## Troubleshooting

1. **Migration Not Found**: Ensure the migration file exists in the correct directory
2. **Database Connection Issues**: Verify the connection string in appsettings.json
3. **Rollback Failed**: Check if the rollback script is correct and all dependencies are removed
4. **Cannot Remove Migration**: Ensure the migration is not already applied to the database

## License

This tool is part of the WITS project and is subject to the project's license terms.
