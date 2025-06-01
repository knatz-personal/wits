// ----------------------------------------------------------------------------------
//  Created by: Nathan Zwelibanzi Khupe
//  @ 2025 Nathan Zwelibanzi Khupe. All rights reserved.
//  Unauthorized copying, distribution, modification, or use strictly prohibited.
//  ----------------------------------------------------------------------------------

namespace WITS.Api.Common;

public static class WiringExtensions
{
    public static void RegisterEndpointDefinitions(this RouteGroupBuilder routeGroupBuilder)
    {
        IEnumerable<IEndpointDefinition> endpointDefinitions = typeof(Program).Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpointDefinition)) && !t.IsAbstract
                                                                      && !t.IsInterface)
            .Select(Activator.CreateInstance).Cast<IEndpointDefinition>();

        foreach (IEndpointDefinition endpointdef in endpointDefinitions)
        {
            endpointdef.Register(routeGroupBuilder);
        }
    }
}
