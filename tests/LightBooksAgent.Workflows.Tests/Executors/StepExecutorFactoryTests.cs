using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Dispatching;
using LightBooksAgent.Workflows.Executors;
using LightBooksAgent.Workflows.Tests.Helpers;

namespace LightBooksAgent.Workflows.Tests.Executors;

public class StepExecutorFactoryTests
{
    [Fact]
    public void Create_ReturnsExecutorWithGivenId()
    {
        var dispatcher = new PublishingStepDispatcher(
            [new StubStepHandler(PublishingStep.Researching)]);
        var factory = new StepExecutorFactory(dispatcher);

        var executor = factory.Create("research", PublishingStep.Researching);

        Assert.Equal("research", executor.Id);
    }

    [Fact]
    public void CreateInitializer_ReturnsInitializeExecutor()
    {
        var factory = new StepExecutorFactory(new PublishingStepDispatcher([]));
        var executor = factory.CreateInitializer();
        Assert.Equal("initialize", executor.Id);
    }

    [Fact]
    public void CreateCompletionExecutor_ReturnsCompleteExecutor()
    {
        var factory = new StepExecutorFactory(new PublishingStepDispatcher([]));
        var executor = factory.CreateCompletionExecutor();
        Assert.Equal("complete", executor.Id);
    }

    [Theory]
    [InlineData(PublishingStep.Researching)]
    [InlineData(PublishingStep.Outlining)]
    [InlineData(PublishingStep.Writing)]
    [InlineData(PublishingStep.TechnicalReview)]
    [InlineData(PublishingStep.EditorialReview)]
    [InlineData(PublishingStep.Publishing)]
    public void Create_ProducesDistinctExecutorIds(PublishingStep step)
    {
        var factory = new StepExecutorFactory(new PublishingStepDispatcher([new StubStepHandler(step)]));
        var executor = factory.Create(step.ToString().ToLowerInvariant(), step);
        Assert.False(string.IsNullOrWhiteSpace(executor.Id));
    }
}
