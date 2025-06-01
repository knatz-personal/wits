// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  @ 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Common;

public class JwtSettings
{
    public required string SecurityKey { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpirationMinutes { get; init; }
}
