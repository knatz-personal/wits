// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

using WITS.Data.Common;

namespace WITS.Data.Entity;

public class Company : BaseEntity<long>
{
    public required string RegistrationNumber { get; set; }
    public required string CompanyName { get; set; }
    public required string AddressLine1 { get; set; }
    public required string AddressLIne2 { get; set; }
    public string? AddressLIne3 { get; set; }
    public string? AddressLine4 { get; set; }
    public required string TelephoneNos { get; set; }
    public required string EmailAddress { get; set; }
    public string? WebSite { get; set; }
    public required string VatNo { get; set; }
    public required string VatCode { get; set; }
    public double VatRate { get; set; }
    public required string LogoPath { get; set; }
}
