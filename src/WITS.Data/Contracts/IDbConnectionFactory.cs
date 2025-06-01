// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using System.Data;

namespace WITS.Data.Contracts;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default);
}
