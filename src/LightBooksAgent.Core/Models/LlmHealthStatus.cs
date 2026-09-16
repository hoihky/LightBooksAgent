namespace LightBooksAgent.Core.Models;

public sealed class LlmHealthStatus
{
    public bool IsHealthy { get; init; }

    public string? ModelName { get; init; }

    public string? Message { get; init; }
}
