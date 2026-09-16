namespace LightBooksAgent.Core.Models;

public sealed class TopicProposal
{
    public string Title { get; set; } = string.Empty;

    public string Angle { get; set; } = string.Empty;

    public string TargetReader { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
