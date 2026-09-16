using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows;
using LightBooksAgent.Workflows.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddLightBooksWorkflows_RegistersCoreServices()
    {
        var services = new ServiceCollection();
        services.AddLightBooksWorkflows();

        Assert.NotNull(services.FirstOrDefault(d => d.ServiceType == typeof(IWorkflowSessionManager)));
        Assert.NotNull(services.FirstOrDefault(d => d.ServiceType == typeof(IPublishingWorkflowFactory)));
        Assert.NotNull(services.FirstOrDefault(d => d.ServiceType == typeof(IPublishingWorkflowRunner)));
        Assert.NotNull(services.FirstOrDefault(d => d.ServiceType == typeof(IWorkflowCheckpointService)));
        Assert.NotNull(services.FirstOrDefault(d => d.ServiceType == typeof(IWorkflowEventProcessor)));
        Assert.NotNull(services.FirstOrDefault(d => d.ServiceType == typeof(IPublishingStepDispatcher)));
    }

    [Fact]
    public void AddLightBooksWorkflows_RegistersAllStepHandlers()
    {
        var services = new ServiceCollection();
        services.AddLightBooksWorkflows();

        var handlerRegistrations = services
            .Where(d => d.ServiceType == typeof(IPublishingStepHandler))
            .ToList();

        Assert.Equal(7, handlerRegistrations.Count);
    }
}
