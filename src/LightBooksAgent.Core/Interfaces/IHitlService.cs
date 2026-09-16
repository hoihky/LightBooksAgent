using LightBooksAgent.Core.Entities;
using LightBooksAgent.Core.Enums;

namespace LightBooksAgent.Core.Interfaces;

public interface IHitlService
{
    Task<ReviewRequest> CreateRequestAsync(
        Guid publishingRunId,
        ReviewGateType gateType,
        string payloadJson,
        CancellationToken cancellationToken = default);

    Task<ReviewRequest> ResolveAsync(
        Guid reviewRequestId,
        ReviewStatus status,
        string? comment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default);
}
