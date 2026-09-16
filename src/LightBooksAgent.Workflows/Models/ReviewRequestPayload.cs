using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Workflows.Models;

public sealed record ReviewRequestPayload(
    ReviewGateType GateType,
    Guid PublishingRunId,
    Guid ArticleProjectId,
    string Summary,
    string Content);
