using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Dispatching;
using LightBooksAgent.Workflows.Executors;
using LightBooksAgent.Workflows.Factory;
using LightBooksAgent.Workflows.Tests.Helpers;

namespace LightBooksAgent.Workflows.Tests.Factory;

public class PublishingWorkflowFactoryTests
{
    private static PublishingWorkflowFactory CreateFactory()
    {
        var handlers = new IPublishingStepHandler[]
        {
            new StubStepHandler(PublishingStep.Researching),
            new StubStepHandler(PublishingStep.Outlining),
            new StubStepHandler(PublishingStep.Writing),
            new StubStepHandler(PublishingStep.Revision),
            new StubStepHandler(PublishingStep.TechnicalReview),
            new StubStepHandler(PublishingStep.EditorialReview),
            new StubStepHandler(PublishingStep.Publishing)
        };

        var dispatcher = new PublishingStepDispatcher(handlers);
        var stepFactory = new StepExecutorFactory(dispatcher);
        var hitlFactory = new HitlGateFactory();
        return new PublishingWorkflowFactory(stepFactory, hitlFactory);
    }

    [Fact]
    public void CreateWorkflow_ReturnsNonNullWorkflow()
    {
        var factory = CreateFactory();
        var workflow = factory.CreateWorkflow();
        Assert.NotNull(workflow);
    }

    [Fact]
    public void CreateWorkflow_DoesNotThrow()
    {
        var factory = CreateFactory();
        var exception = Record.Exception(() => factory.CreateWorkflow());
        Assert.Null(exception);
    }

    [Fact]
    public void CreateWorkflow_CanBeInvokedMultipleTimes()
    {
        var factory = CreateFactory();
        var first = factory.CreateWorkflow();
        var second = factory.CreateWorkflow();
        Assert.NotNull(first);
        Assert.NotNull(second);
    }
}
