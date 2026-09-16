using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Interfaces;

public interface IActivityLogger
{
    Task LogAsync(
        Guid publishingRunId,
        string agentName,
        ActivityType activityType,
        string message,
        string? url = null,
        int? durationMs = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentActivity>> GetRecentAsync(
        Guid? publishingRunId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
