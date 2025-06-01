// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Common;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalRecords,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
}
