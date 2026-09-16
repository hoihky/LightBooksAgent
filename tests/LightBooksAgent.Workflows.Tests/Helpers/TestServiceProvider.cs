using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LightBooksAgent.Workflows.Tests.Helpers;

internal static class TestServiceProvider
{
    public static IServiceScopeFactory Create(AppDbContext db)
    {
        var hitlService = Substitute.For<IHitlService>();
        var activityLogger = Substitute.For<IActivityLogger>();

        var services = new ServiceCollection();
        services.AddSingleton(db);
        services.AddSingleton(hitlService);
        services.AddSingleton(activityLogger);

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IServiceScopeFactory>();
    }
}
