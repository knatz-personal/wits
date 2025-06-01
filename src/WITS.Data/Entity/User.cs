// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Data.Common;

namespace WITS.Data.Entity;

public class User : BaseEntity<long>
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string FullName { get; set; }
    public int Status { get; set; }
    public int RoleLevel { get; set; }
    public required string TelephoneNos { get; set; }
    public int OrganisationId { get; set; }
    public required string Organisation { get; set; }
    public required string EmailAddress { get; set; }
}
