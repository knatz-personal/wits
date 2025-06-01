// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Data.Common;

namespace WITS.Data.Entity;

public class TicketStatus : BaseEntity<long>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
