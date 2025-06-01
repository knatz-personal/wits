// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Data.Entity;

public partial class Ticket
{
    public string TicketRef => $"{ProjectCode}-{Id:D8}";
}
