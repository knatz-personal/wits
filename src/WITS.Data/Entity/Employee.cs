// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Data.Common;

namespace WITS.Data.Entity;

public class Employee : BaseEntity<long>
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int Status { get; set; }
}
