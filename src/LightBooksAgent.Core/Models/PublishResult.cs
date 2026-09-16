namespace LightBooksAgent.Core.Models;

public sealed class PublishResult
{
    public bool Success { get; init; }

    public string? MarkdownPath { get; init; }

    public string? HtmlOutputPath { get; init; }

    public IReadOnlyList<string> Errors { get; init; } = [];
}
