using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Workflows.Abstractions;
using LightBooksAgent.Workflows.Dispatching;
using LightBooksAgent.Workflows.Executors;
using LightBooksAgent.Workflows.Factory;
using LightBooksAgent.Workflows.Hosting;
using LightBooksAgent.Workflows.Models;
using LightBooksAgent.Workflows.Orchestration;
using LightBooksAgent.Workflows.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace LightBooksAgent.Workflows.Tests.Orchestration;

public class MafPublishingWorkflowRunnerTests
{
    [Fact]
    public async Task StartAsync_ThrowsWhenTopicNotConfirmed()
    {
        await using var db = TestDbContextFactory.Create();
        var project = new ArticleProject { Title = "Draft", Status = ArticleStatus.Draft };
        db.ArticleProjects.Add(project);
        await db.SaveChangesAsync();

        var runner = CreateRunner(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => runner.StartAsync(project.Id));
    }

    [Fact]
    public async Task StartAsync_CreatesRunAndRegistersSession()
    {
        await using var db = TestDbContextFactory.Create();
        var project = new ArticleProject
        {
            Title = "MAF Article",
            ConfirmedTopic = "Sequential workflows",
            Status = ArticleStatus.Draft
        };
        db.ArticleProjects.Add(project);
        await db.SaveChangesAsync();

        var sessionManager = new WorkflowSessionManager();
        var runner = CreateRunner(db, sessionManager);
        var run = await runner.StartAsync(project.Id);

        Assert.NotEqual(Guid.Empty, run.Id);
        Assert.Equal(project.Id, run.ArticleProjectId);
        Assert.Equal(AgentRunStatus.Running, run.AgentStatus);
        Assert.Equal(PublishingStep.Researching, run.CurrentStep);
        Assert.Equal(ArticleStatus.InProgress, (await db.ArticleProjects.FindAsync(project.Id))!.Status);
        Assert.True(sessionManager.TryGet(run.Id, out var handle));
        Assert.NotNull(handle);
    }

    [Fact]
    public async Task GetRunAsync_ReturnsNullForUnknownId()
    {
        await using var db = TestDbContextFactory.Create();
        var runner = CreateRunner(db);

        var run = await runner.GetRunAsync(Guid.NewGuid());

        Assert.Null(run);
    }

    [Fact]
    public async Task GetActiveRunsAsync_FiltersByActiveStatuses()
    {
        await using var db = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        db.ArticleProjects.Add(new ArticleProject { Id = projectId, Title = "T" });

        db.PublishingRuns.AddRange(
            new PublishingRun { ArticleProjectId = projectId, AgentStatus = AgentRunStatus.Running },
            new PublishingRun { ArticleProjectId = projectId, AgentStatus = AgentRunStatus.WaitingForHuman },
            new PublishingRun { ArticleProjectId = projectId, AgentStatus = AgentRunStatus.Completed },
            new PublishingRun { ArticleProjectId = projectId, AgentStatus = AgentRunStatus.Stopped });

        await db.SaveChangesAsync();

        var runner = CreateRunner(db);
        var active = await runner.GetActiveRunsAsync();

        Assert.Equal(2, active.Count);
        Assert.All(active, r => Assert.True(
            r.AgentStatus is AgentRunStatus.Running or AgentRunStatus.WaitingForHuman or AgentRunStatus.Paused));
    }

    [Fact]
    public async Task AdvanceAfterReviewAsync_ThrowsWhenNoPendingSession()
    {
        await using var db = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        db.ArticleProjects.Add(new ArticleProject { Id = projectId, Title = "T" });
        var run = new PublishingRun { ArticleProjectId = projectId, AgentStatus = AgentRunStatus.WaitingForHuman };
        db.PublishingRuns.Add(run);
        await db.SaveChangesAsync();

        var runner = CreateRunner(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => runner.AdvanceAfterReviewAsync(run.Id, ReviewGateType.DraftReview, true));
    }

    [Fact]
    public async Task PauseAsync_UpdatesRunStatus()
    {
        await using var db = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        db.ArticleProjects.Add(new ArticleProject { Id = projectId, Title = "T" });
        var run = new PublishingRun { ArticleProjectId = projectId, AgentStatus = AgentRunStatus.Running };
        db.PublishingRuns.Add(run);
        await db.SaveChangesAsync();

        var runner = CreateRunner(db);
        await runner.PauseAsync(run.Id);

        var updated = await db.PublishingRuns.FindAsync(run.Id);
        Assert.Equal(AgentRunStatus.Paused, updated!.AgentStatus);
    }

    [Fact]
    public async Task StopAsync_UpdatesRunStatus()
    {
        await using var db = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        db.ArticleProjects.Add(new ArticleProject { Id = projectId, Title = "T" });
        var run = new PublishingRun { ArticleProjectId = projectId, AgentStatus = AgentRunStatus.Running };
        db.PublishingRuns.Add(run);
        await db.SaveChangesAsync();

        var runner = CreateRunner(db);
        await runner.StopAsync(run.Id);

        var updated = await db.PublishingRuns.FindAsync(run.Id);
        Assert.Equal(AgentRunStatus.Stopped, updated!.AgentStatus);
    }

    [Fact]
    public async Task AdvanceAfterReviewAsync_IncrementsRevisionCountOnDraftRejection()
    {
        await using var db = TestDbContextFactory.Create();
        var projectId = Guid.NewGuid();
        db.ArticleProjects.Add(new ArticleProject { Id = projectId, Title = "T" });
        var run = new PublishingRun
        {
            ArticleProjectId = projectId,
            AgentStatus = AgentRunStatus.WaitingForHuman,
            RevisionCount = 1
        };
        db.PublishingRuns.Add(run);
        db.ReviewRequests.Add(new ReviewRequest
        {
            PublishingRunId = run.Id,
            GateType = ReviewGateType.DraftReview,
            HumanComment = "Expand intro",
            Status = ReviewStatus.Approved
        });
        await db.SaveChangesAsync();

        var sessionManager = new WorkflowSessionManager();
        var handle = TestMafObjects.CreateSessionHandle(run.Id, TestMafObjects.CreateExternalRequest());
        sessionManager.Register(handle);

        var runner = CreateRunner(db, sessionManager);

        // Host submission fails with uninitialized MAF request, but DB state is updated first.
        await Assert.ThrowsAnyAsync<Exception>(
            () => runner.AdvanceAfterReviewAsync(run.Id, ReviewGateType.DraftReview, false));

        var updated = await db.PublishingRuns.FindAsync(run.Id);
        Assert.Equal(2, updated!.RevisionCount);
        Assert.Equal(AgentRunStatus.Running, updated.AgentStatus);
    }

    private static MafPublishingWorkflowRunner CreateRunner(
        AppDbContext db,
        WorkflowSessionManager? sessionManager = null)
    {
        sessionManager ??= new WorkflowSessionManager();
        var checkpointProvider = TestCheckpointProvider.Create();
        var checkpointService = new WorkflowCheckpointService(
            db,
            checkpointProvider,
            NullLogger<WorkflowCheckpointService>.Instance);

        var handlers = new IPublishingStepHandler[]
        {
            new StubStepHandler(PublishingStep.ResearchUrlProposing, s => { s.ProposedResearchUrls = ["https://example.com"]; return s; }),
            new StubStepHandler(PublishingStep.Researching, s => { s.ResearchBrief = "brief"; return s; }),
            new StubStepHandler(PublishingStep.Outlining, s => { s.OutlineMarkdown = "# Outline"; return s; }),
            new StubStepHandler(PublishingStep.Writing, s => { s.LatestDraft = "draft"; return s; }),
            new StubStepHandler(PublishingStep.Revision, s => s),
            new StubStepHandler(PublishingStep.TechnicalReview, s => s),
            new StubStepHandler(PublishingStep.EditorialReview, s => s),
            new StubStepHandler(PublishingStep.Publishing, s => { s.IsCompleted = true; return s; })
        };

        var dispatcher = new PublishingStepDispatcher(handlers);
        var stepFactory = new StepExecutorFactory(dispatcher);
        var workflowFactory = new PublishingWorkflowFactory(stepFactory, new HitlGateFactory());
        var eventProcessor = new WorkflowEventProcessor(
            TestServiceProvider.Create(db),
            sessionManager,
            checkpointService,
            NullLogger<WorkflowEventProcessor>.Instance);

        var host = new PublishingWorkflowHost(
            workflowFactory,
            checkpointService,
            sessionManager,
            eventProcessor,
            NullLogger<PublishingWorkflowHost>.Instance);

        return new MafPublishingWorkflowRunner(
            db,
            host,
            sessionManager,
            checkpointService,
            Substitute.For<IMemoryService>());
    }
}
