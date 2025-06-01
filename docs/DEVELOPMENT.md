# WITS Development Guide

## Development Environment Setup

### Required Tools

-   Visual Studio 2022 or later
-   .NET 9.0 SDK
-   SQLite
-   Git

### Editor Configuration

The project includes an `.editorconfig` file that defines coding style and formatting rules. Ensure your IDE respects these settings:

-   4 spaces for indentation
-   UTF-8 file encoding
-   Windows line endings (CRLF)
-   Trailing whitespace trimming

## Project Structure

### WITS.Api

The API project follows a feature-based structure:

```
WITS.Api/
├── Features/           # not a namespace provider
│   ├── Common/         # Shared components
│   └── Tickets/        # Ticket-related endpoints
|   |__ ...
├── Program.cs          # Application entry point
└── appsettings.json   # Configuration
```

### WITS.Data

Data Access Layer with repository pattern implementation:

```
WITS.Data/
├── Common/            # Shared data types
├── Contracts/         # Interfaces
├── Entity/           # Domain entities
├── Factory/          # Database factories
├── Repository/       # Data access implementations
└── SQLScripts/       # Database schema
```

## Code Organization

### Domain Entities

-   Place in WITS.Data/Entity
-   Use proper C# naming conventions
-   Include XML documentation
-   Implement appropriate interfaces

Example:

```csharp
public class Ticket : BaseEntity<long>
{
    public string ProjectCode { get; set; }
    public string Summary { get; set; }
    // ...
}
```

### Repository Pattern

-   Define interfaces in Contracts folder
-   Implement in Repository folder
-   Use dependency injection
-   Include unit tests

### API Endpoints

-   Group by feature in Features folder
-   Use minimal API syntax
-   Include OpenAPI documentation
-   Implement proper validation

## Database

### Schema Management

-   SQL scripts in SQLScripts folder
-   Use migrations for changes
-   Document complex queries
-   Include indexes for performance

### Best Practices

1. Use parameters to prevent SQL injection
2. Implement proper transaction handling
3. Use appropriate data types
4. Include foreign key constraints
5. Implement soft delete where appropriate

## Testing

### Unit Tests

-   Test each component in isolation
-   Use appropriate mocking
-   Follow Arrange-Act-Assert pattern
-   Cover edge cases

### Integration Tests

-   Test component interactions
-   Use test database
-   Clean up test data
-   Verify end-to-end flows

## Performance Considerations

### Database

-   Use appropriate indexes
-   Implement paging
-   Optimize queries
-   Monitor query performance

### API

-   Cache when appropriate
-   Use async/await properly
-   Implement rate limiting
-   Return appropriate data sizes

## Security

### Authentication

-   Use proper token validation
-   Implement refresh tokens
-   Secure sensitive data
-   Log security events

### Authorization

-   Implement role-based access
-   Validate at API level
-   Use proper HTTP methods
-   Implement proper CORS

## Logging

### Guidelines

1. Use appropriate log levels
2. Include relevant context
3. Avoid sensitive information
4. Use structured logging

### Log Levels

-   ERROR: Application errors
-   WARN: Warning conditions
-   INFO: Informational messages
-   DEBUG: Debugging information

## Error Handling

### Best Practices

1. Use proper exception types
2. Include stack traces
3. Log appropriately
4. Return proper status codes

### Exception Handling

```csharp
try
{
    // Operation
}
catch (SpecificException ex)
{
    // Handle specific case
    logger.LogError(ex, "Error message");
    throw;
}
```

## Deployment

### Environment Configuration

-   Use appropriate settings per environment
-   Secure sensitive configuration
-   Use proper service accounts
-   Configure proper logging

### Health Monitoring

-   Implement health checks
-   Monitor performance
-   Set up alerts
-   Track error rates

## Contributing

### Process

1. Create feature branch
2. Write tests
3. Implement changes
4. Update documentation
5. Submit pull request

### Code Review

-   Follow style guidelines
-   Check test coverage
-   Review performance
-   Verify security
