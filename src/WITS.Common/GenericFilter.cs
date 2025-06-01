// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Common;

public record GenericFilter(
    Dictionary<string, FilterCriteria>? Filters = null,
    string? SortColumn = null,
    string SortOrder = "ASC",
    int PageNumber = 1,
    int PageSize = 10
);
