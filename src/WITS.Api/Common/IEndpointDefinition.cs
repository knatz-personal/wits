// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  � 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Api.Common;

public interface IEndpointDefinition
{
    void Register(RouteGroupBuilder routeBuilder);
}
