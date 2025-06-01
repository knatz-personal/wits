// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Data.Common;

public abstract class BaseEntity<TDataType>
{
    public required TDataType Id { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
