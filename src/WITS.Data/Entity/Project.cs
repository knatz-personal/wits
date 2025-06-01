// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Data.Common;

namespace WITS.Data.Entity;

public class Project : BaseEntity<long>
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int Type { get; set; }
    public required string AdminUserName { get; set; }
    public bool EmailOnNewDefect { get; set; }
    public int OrganisationId { get; set; }
    public string? ProjectGroup { get; set; }
}
