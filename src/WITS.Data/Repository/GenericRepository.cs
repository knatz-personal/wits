// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using System.Data;
using System.Text.Json;
using Dapper;
using WITS.Common;
using WITS.Data.Common;
using WITS.Data.Contracts;

namespace WITS.Data.Repository;

public class GenericRepository<T, TDataType> : IGenericRepository<T, TDataType>
    where T : BaseEntity<TDataType>
{
    private static readonly string TypeName = typeof(T).Name;
    private readonly List<string> _columnNames;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly string _tableName;

    public GenericRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _tableName = $"{TypeName}";
        _columnNames = typeof(T)
            .GetProperties()
            .Where(p => p.Name != "Id")
            .Select(p => p.Name)
            .ToList();
    }

    public async Task<T?> GetByIdAsync(TDataType id)
    {
        string query = $"SELECT * FROM {_tableName} WHERE Id = @Id";
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<T>(query, new { Id = id });
    }

    public async Task<int> InsertAsync(T model)
    {
        string query =
            $"INSERT INTO {_tableName} ({string.Join(',', _columnNames)}) " +
            $"VALUES (@{string.Join(", @", _columnNames)});" +
            "SELECT CAST(SCOPE_IDENTITY() as int)";

        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
        int id = await connection.QueryFirstOrDefaultAsync<int>(query, model);
        return id;
    }

    public async Task<bool> UpdateAsync(T model)
    {
        IEnumerable<string> setValues = _columnNames.Select(prop => $"{prop} = @{prop}");
        string query = $"UPDATE {_tableName} SET {string.Join(", ", setValues)} WHERE {TypeName}Id = @Id";

        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
        int result = await connection.ExecuteAsync(query, model);
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(TDataType id)
    {
        string query = $"DELETE FROM {_tableName} WHERE {TypeName}Id = @Id";
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
        int result = await connection.ExecuteAsync(query, new { Id = id });
        return result > 0;
    }

    public async Task<PagedResult<T>> GetAllAsync(int pageNumber = 1, int pageSize = 100)
    {
        try
        {
            using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

            string countQuery = $"SELECT COUNT(*) FROM {_tableName} WHERE IsDeleted = 0";
            int totalRecords = await connection.ExecuteScalarAsync<int>(countQuery);

            int pageStart = (pageNumber - 1) * pageSize;

            string query = $"""

                                            SELECT *
                                            FROM {_tableName}
                                            WHERE IsDeleted = 0
                                            ORDER BY Id
                                            LIMIT @PageStart, @PageSize
                            """;

            DynamicParameters parameters = new();
            parameters.Add("@PageStart", pageStart);
            parameters.Add("@PageSize", pageSize);

            IReadOnlyList<T> items = (await connection.QueryAsync<T>(query, parameters)).ToList();

            return new PagedResult<T>(
                items,
                totalRecords,
                pageNumber,
                pageSize
            );
        }
        catch (Exception)
        {
            return new PagedResult<T>(
                Array.Empty<T>(),
                0,
                pageNumber,
                pageSize
            );
        }
    }

    public async Task<PagedResult<T>> QueryAsync(GenericFilter filter)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        DynamicParameters parameters = new();
        List<string> whereConditions = new();
        int paramIndex = 0;

        // ---------------------------------------------
        // 1. Build filter conditions
        // ---------------------------------------------
        if (filter.Filters is { Count: > 0 })
        {
            foreach ((string property, FilterCriteria criteria) in filter.Filters)
            {
                if (!_columnNames.Any(c => c.Equals(property, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                string op = criteria.Operator?.ToUpperInvariant() ?? "EQUALS";
                string paramName = $"@p{paramIndex++}";
                object? value = criteria.Value is JsonElement json
                    ? json.ToPrimitive()
                    : criteria.Value;

                switch (op)
                {
                    case "CONTAINS":
                        whereConditions.Add($"{property} LIKE {paramName}");
                        parameters.Add(paramName, $"%{value}%");
                        break;

                    case "GT":
                    case ">":
                        whereConditions.Add($"{property} > {paramName}");
                        parameters.Add(paramName, value);
                        break;

                    case "GTE":
                    case ">=":
                        whereConditions.Add($"{property} >= {paramName}");
                        parameters.Add(paramName, value);
                        break;

                    case "LT":
                    case "<":
                        whereConditions.Add($"{property} < {paramName}");
                        parameters.Add(paramName, value);
                        break;

                    case "LTE":
                    case "<=":
                        whereConditions.Add($"{property} <= {paramName}");
                        parameters.Add(paramName, value);
                        break;

                    case "EQUALS":
                    case "=":
                        whereConditions.Add($"{property} = {paramName}");
                        parameters.Add(paramName, value);
                        break;
                    default:
                        break;
                }
            }
        }

        string whereClause = "WHERE IsDeleted = 0";

        if (whereConditions.Count > 0)
        {
            whereClause += " AND " + string.Join(" AND ", whereConditions);
        }

        // ---------------------------------------------
        // 2. Build COUNT query
        // ---------------------------------------------
        string countSql = $"SELECT COUNT(*) FROM {_tableName} {whereClause}";
        int totalRecords = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // ---------------------------------------------
        // 3. Build main SELECT query
        // ---------------------------------------------
        string sortColumn = !string.IsNullOrWhiteSpace(filter.SortColumn) && _columnNames.Contains(filter.SortColumn)
            ? filter.SortColumn
            : "Id";

        string sortOrder = filter.SortOrder?.ToUpperInvariant() == "DESC" ? "DESC" : "ASC";

        int offset = (filter.PageNumber - 1) * filter.PageSize;

        string sql = $"""
                          SELECT * FROM {_tableName}
                          {whereClause}
                          ORDER BY {sortColumn} {sortOrder}
                          LIMIT @Offset, @PageSize
                      """;

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", filter.PageSize);

        List<T> items = (await connection.QueryAsync<T>(sql, parameters)).ToList();

        return new PagedResult<T>(
            items,
            totalRecords,
            filter.PageNumber,
            filter.PageSize
        );
    }

    public async Task<PagedResult<T>> SearchAsync(SearchParams searchParams)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        DynamicParameters parameters = new();
        parameters.Add("@SearchTerm", searchParams.SearchString);
        parameters.Add("@OrganisationId", searchParams.OrganisationId);
        parameters.Add("@Offset", (searchParams.PageNumber - 1) * searchParams.PageSize);
        parameters.Add("@PageSize", searchParams.PageSize);

        string ftsTable = $"{_tableName}FTS";

        // Count query
        string countSql = $"""
                               SELECT COUNT(*)
                               FROM {_tableName} AS main
                               JOIN {ftsTable} AS fts ON fts.rowid = main.Id
                               WHERE fts MATCH @SearchTerm
                               AND main.IsDeleted = 0
                               AND main.OrganisationId = @OrganisationId
                           """;

        int totalRecords = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        // Main query with relevance
        string sql = $"""
                          SELECT main.*, bm25(fts) AS Relevance
                          FROM {_tableName} AS main
                          JOIN {ftsTable} AS fts ON fts.rowid = main.Id
                          WHERE fts MATCH @SearchTerm
                          AND main.IsDeleted = 0
                          AND main.OrganisationId = @OrganisationId
                          ORDER BY Relevance ASC, main.Id DESC
                          LIMIT @PageSize OFFSET @Offset
                      """;

        List<T> results = (await connection.QueryAsync<T>(sql, parameters)).ToList();

        return new PagedResult<T>(
            results,
            totalRecords,
            searchParams.PageNumber,
            searchParams.PageSize
        );
    }
}
