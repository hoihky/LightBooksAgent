using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Dispatching;
using LightBooksAgent.Workflows.Executors;
using LightBooksAgent.Workflows.Factory;
using LightBooksAgent.Workflows.Hosting;
using LightBooksAgent.Workflows.Orchestration;
using LightBooksAgent.Workflows.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Workflows;

public static class DependencyInjection
{
    public static IServiceCollection AddLightBooksWorkflows(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowSessionManager, WorkflowSessionManager>();
        services.AddSingleton<WorkflowCheckpointManagerProvider>();
        services.AddScoped<IWorkflowCheckpointService, WorkflowCheckpointService>();
        services.AddScoped<IPublishingWorkflowFactory, PublishingWorkflowFactory>();
        services.AddScoped<StepExecutorFactory>();
        services.AddScoped<HitlGateFactory>();
        services.AddScoped<PublishingWorkflowHost>();

        services.AddScoped<IWorkflowEventProcessor, WorkflowEventProcessor>();
        services.AddScoped<IPublishingStepDispatcher, PublishingStepDispatcher>();

        services.AddScoped<IPublishingStepHandler, ResearchStepHandler>();
        services.AddScoped<IPublishingStepHandler, OutlineStepHandler>();
        services.AddScoped<IPublishingStepHandler, WritingStepHandler>();
        services.AddScoped<IPublishingStepHandler, RevisionStepHandler>();
        services.AddScoped<IPublishingStepHandler, TechnicalReviewStepHandler>();
        services.AddScoped<IPublishingStepHandler, EditorialStepHandler>();
        services.AddScoped<IPublishingStepHandler, PublishStepHandler>();

        services.AddScoped<WritingStepHandler>();
        services.AddScoped<IPublishingWorkflowRunner, MafPublishingWorkflowRunner>();

        return services;
    }
}
