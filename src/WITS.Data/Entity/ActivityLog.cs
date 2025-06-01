// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Data.Common;

namespace WITS.Data.Entity;

public class ActivityLog : BaseEntity<long>
{
    public int TicketId { get; set; }
    public int TypeId { get; set; }
    public DateTime EntryDate { get; set; }
    public required string LoggedBy { get; set; }
    public required string Description { get; set; }
    public string? FilePath { get; set; }
}
