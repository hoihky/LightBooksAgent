namespace LightBooksAgent.Workflows.Models;

public sealed record WorkflowStartInput(Guid PublishingRunId, Guid ArticleProjectId);
