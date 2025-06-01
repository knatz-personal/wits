// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Common;

public record FilterCriteria(
    object Value,
    string Operator = "Equals" // Could be "Equals", "Contains", "StartsWith", "GT", "GTE", "LT", "LTE", etc.
);
