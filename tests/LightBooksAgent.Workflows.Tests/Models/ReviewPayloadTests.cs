using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Models;

namespace LightBooksAgent.Workflows.Tests.Models;

public class ReviewPayloadTests
{
    [Fact]
    public void ReviewRequestPayload_StoresAllFields()
    {
        var runId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var payload = new ReviewRequestPayload(
            ReviewGateType.DraftReview,
            runId,
            projectId,
            "Draft ready",
            "# Heading");

        Assert.Equal(ReviewGateType.DraftReview, payload.GateType);
        Assert.Equal(runId, payload.PublishingRunId);
        Assert.Equal(projectId, payload.ArticleProjectId);
        Assert.Equal("Draft ready", payload.Summary);
        Assert.Equal("# Heading", payload.Content);
    }

    [Fact]
    public void ReviewResponsePayload_StoresApprovalAndComment()
    {
        var approved = new ReviewResponsePayload(true, "Looks good");
        var rejected = new ReviewResponsePayload(false, "Needs work");

        Assert.True(approved.Approved);
        Assert.Equal("Looks good", approved.Comment);
        Assert.False(rejected.Approved);
        Assert.Equal("Needs work", rejected.Comment);
    }

    [Fact]
    public void WorkflowStartInput_StoresIdentifiers()
    {
        var runId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var input = new WorkflowStartInput(runId, projectId);

        Assert.Equal(runId, input.PublishingRunId);
        Assert.Equal(projectId, input.ArticleProjectId);
    }
}
