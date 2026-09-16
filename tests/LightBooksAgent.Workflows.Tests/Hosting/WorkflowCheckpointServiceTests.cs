using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Workflows.Hosting;
using LightBooksAgent.Workflows.Tests.Helpers;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging.Abstractions;

namespace LightBooksAgent.Workflows.Tests.Hosting;

public class WorkflowCheckpointServiceTests
{
    [Fact]
    public async Task PersistRunCheckpointAsync_UpdatesRunRecord()
    {
        await using var db = TestDbContextFactory.Create();
        var run = SeedRun(db);
        var provider = TestCheckpointProvider.Create();
        var service = new WorkflowCheckpointService(db, provider, NullLogger<WorkflowCheckpointService>.Instance);
        var checkpoint = new CheckpointInfo("session-1", "checkpoint-abc");

        await service.PersistRunCheckpointAsync(run.Id, checkpoint);

        var updated = await db.PublishingRuns.FindAsync(run.Id);
        Assert.NotNull(updated);
        Assert.Equal("checkpoint-abc", updated!.CheckpointId);
        Assert.Equal("session-1", updated.WorkflowRunId);
    }

    [Fact]
    public async Task PersistRunCheckpointAsync_IgnoresMissingRun()
    {
        await using var db = TestDbContextFactory.Create();
        var provider = TestCheckpointProvider.Create();
        var service = new WorkflowCheckpointService(db, provider, NullLogger<WorkflowCheckpointService>.Instance);

        await service.PersistRunCheckpointAsync(Guid.NewGuid(), new CheckpointInfo("s", "c"));

        Assert.Empty(db.PublishingRuns);
    }

    [Fact]
    public async Task GetLatestRunCheckpointAsync_ReturnsNullWhenUnset()
    {
        await using var db = TestDbContextFactory.Create();
        var run = SeedRun(db);
        var provider = TestCheckpointProvider.Create();
        var service = new WorkflowCheckpointService(db, provider, NullLogger<WorkflowCheckpointService>.Instance);

        var checkpoint = await service.GetLatestRunCheckpointAsync(run.Id);

        Assert.Null(checkpoint);
    }

    [Fact]
    public async Task GetLatestRunCheckpointAsync_ReturnsPersistedCheckpoint()
    {
        await using var db = TestDbContextFactory.Create();
        var run = SeedRun(db);
        var provider = TestCheckpointProvider.Create();
        var service = new WorkflowCheckpointService(db, provider, NullLogger<WorkflowCheckpointService>.Instance);
        var expected = new CheckpointInfo("session-42", "cp-99");

        await service.PersistRunCheckpointAsync(run.Id, expected);

        var checkpoint = await service.GetLatestRunCheckpointAsync(run.Id);

        Assert.NotNull(checkpoint);
        Assert.Equal(expected.SessionId, checkpoint!.SessionId);
        Assert.Equal(expected.CheckpointId, checkpoint.CheckpointId);
    }

    [Fact]
    public void Manager_ExposesProviderCheckpointManager()
    {
        var provider = TestCheckpointProvider.Create();
        using var db = TestDbContextFactory.Create();
        var service = new WorkflowCheckpointService(db, provider, NullLogger<WorkflowCheckpointService>.Instance);

        Assert.Same(provider.Manager, service.Manager);
    }

    private static PublishingRun SeedRun(AppDbContext db)
    {
        var project = new ArticleProject
        {
            Title = "Test Article",
            ConfirmedTopic = "MAF workflows",
            Status = ArticleStatus.InProgress
        };
        db.ArticleProjects.Add(project);

        var run = new PublishingRun
        {
            ArticleProjectId = project.Id,
            AgentStatus = AgentRunStatus.Running,
            CurrentStep = PublishingStep.Researching
        };
        db.PublishingRuns.Add(run);
        db.SaveChanges();
        return run;
    }
}
