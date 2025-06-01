// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Data.Common;

namespace WITS.Data.Entity;

public class UserProject : BaseEntity<long>
{
    public required string Username { get; set; }
    public required string ProjectCode { get; set; }
    public int ProjectId { get; set; }
    public int UserId { get; set; }
}
