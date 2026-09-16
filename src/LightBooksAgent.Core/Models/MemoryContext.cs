namespace LightBooksAgent.Core.Models;

public sealed class MemoryContext
{
    public IReadOnlyList<string> Lessons { get; init; } = [];

    public string FormattedBlock =>
        Lessons.Count == 0
            ? string.Empty
            : "## Relevant past experience\n" + string.Join("\n", Lessons.Select(l => $"- {l}"));
}
