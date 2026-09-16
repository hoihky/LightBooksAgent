using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Application.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LightBooksAgent.Application.Services;

public sealed class ActivityService(AppDbContext db) : IActivityLogger
{
    public async Task LogAsync(
        Guid publishingRunId,
        string agentName,
        ActivityType activityType,
        string message,
        string? url = null,
        int? durationMs = null,
        CancellationToken cancellationToken = default)
    {
        var activity = new AgentActivity
        {
            PublishingRunId = publishingRunId,
            AgentName = agentName,
            ActivityType = activityType,
            Message = message,
            Url = url,
            DurationMs = durationMs
        };

        db.AgentActivities.Add(activity);

        var run = await db.PublishingRuns.FindAsync([publishingRunId], cancellationToken);
        if (run is not null)
        {
            run.CurrentAgentName = agentName;
            run.CurrentActivity = message;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgentActivity>> GetRecentAsync(
        Guid? publishingRunId = null,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var query = db.AgentActivities.AsQueryable();

        if (publishingRunId.HasValue)
        {
            query = query.Where(a => a.PublishingRunId == publishingRunId.Value);
        }

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
