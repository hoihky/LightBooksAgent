using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Dispatching;
using LightBooksAgent.Workflows.Models;
using LightBooksAgent.Workflows.Tests.Helpers;

namespace LightBooksAgent.Workflows.Tests.Dispatching;

public class PublishingStepDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_InvokesRegisteredHandler()
    {
        var state = PublishingWorkflowState.CreateStart(Guid.NewGuid(), Guid.NewGuid());
        var handler = new StubStepHandler(
            PublishingStep.Researching,
            s =>
            {
                s.ResearchBrief = "brief";
                return s;
            });

        var dispatcher = new PublishingStepDispatcher([handler]);
        var result = await dispatcher.DispatchAsync(PublishingStep.Researching, state);

        Assert.Equal("brief", result.ResearchBrief);
    }

    [Fact]
    public async Task DispatchAsync_ThrowsWhenHandlerMissing()
    {
        var dispatcher = new PublishingStepDispatcher([]);
        var state = PublishingWorkflowState.CreateStart(Guid.NewGuid(), Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => dispatcher.DispatchAsync(PublishingStep.Writing, state));

        Assert.Contains("Writing", exception.Message);
    }

    [Fact]
    public async Task DispatchAsync_RoutesToCorrectHandlerAmongMany()
    {
        var research = new StubStepHandler(PublishingStep.Researching, s => { s.ResearchBrief = "r"; return s; });
        var outline = new StubStepHandler(PublishingStep.Outlining, s => { s.OutlineMarkdown = "o"; return s; });
        var dispatcher = new PublishingStepDispatcher([research, outline]);
        var state = PublishingWorkflowState.CreateStart(Guid.NewGuid(), Guid.NewGuid());

        var outlineResult = await dispatcher.DispatchAsync(PublishingStep.Outlining, state);

        Assert.Equal("o", outlineResult.OutlineMarkdown);
        Assert.Null(outlineResult.ResearchBrief);
    }
}
