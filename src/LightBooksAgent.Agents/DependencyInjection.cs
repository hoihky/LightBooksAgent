using LightBooksAgent.Agents.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Agents;

public static class DependencyInjection
{
    public static IServiceCollection AddLightBooksAgents(this IServiceCollection services)
    {
        services.AddScoped<AgentRunner>();
        return services;
    }
}
