using LightBooksAgent.Agents.Prompts;
using LightBooksAgent.Core.Constants;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Models;
using LightBooksAgent.Core.Utilities;

namespace LightBooksAgent.Agents.Runner;

public sealed class AgentRunner(
    ILocalLlmClient llmClient,
    IMemoryService memoryService,
    IUrlFetcher urlFetcher,
    IActivityLogger activityLogger)
{
    public Task<IReadOnlyList<string>> ProposeResearchTargetUrlsAsync(
        Guid runId,
        string seedKeywords,
        CancellationToken cancellationToken = default) =>
        ProposeResearchTargetUrlsInternalAsync(runId, seedKeywords, cancellationToken);

    public async Task<string> RunResearchAsync(
        Guid runId,
        ArticleCategory category,
        string topic,
        string seedKeywords,
        IReadOnlyList<string> approvedUrls,
        CancellationToken cancellationToken = default)
    {
        await LogStart(runId, AgentNames.Research, "Researching topic", cancellationToken);

        var memory = await memoryService.RetrieveContextAsync(
            AgentNames.Research,
            category,
            topic,
            cancellationToken);

        var sourceSummaries = new List<string>();
        foreach (var url in approvedUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            var fetch = await urlFetcher.FetchAsync(url, cancellationToken);
            await activityLogger.LogAsync(
                runId,
                AgentNames.Research,
                fetch.Success ? ActivityType.UrlFetched : ActivityType.AgentError,
                fetch.Success ? $"Fetched {url}" : $"Failed to fetch {url}: {fetch.Error}",
                url,
                cancellationToken: cancellationToken);

            if (fetch.Success)
            {
                sourceSummaries.Add($"URL: {url}\nTitle: {fetch.Title}\nExcerpt: {fetch.Content[..Math.Min(1500, fetch.Content.Length)]}");
            }
        }

        var userPrompt = $"""
            Topic: {topic}
            Category: {category}
            Keywords: {seedKeywords}

            Source material:
            {string.Join("\n\n", sourceSummaries)}

            Create a research brief with:
            1. Key concepts to cover
            2. Recommended article angle
            3. Source notes with URLs
            4. Gaps that need more research
            """;

        var systemPrompt = AppendMemory(AgentPrompts.ResearchSystem, memory);
        var result = await llmClient.CompleteAsync(systemPrompt, userPrompt, cancellationToken);

        await memoryService.StoreEpisodicAsync(
            Guid.Empty,
            AgentNames.Research,
            $"Research brief for '{topic}': {result[..Math.Min(500, result.Length)]}",
            cancellationToken);

        await LogComplete(runId, AgentNames.Research, "Research brief created", cancellationToken);
        return result;
    }

    private async Task<IReadOnlyList<string>> ProposeResearchTargetUrlsInternalAsync(
        Guid runId,
        string seedKeywords,
        CancellationToken cancellationToken)
    {
        await LogStart(runId, AgentNames.Research, "Proposing research target URLs", cancellationToken);

        var urls = ResearchUrlParser.ParseFromSeedKeywords(seedKeywords);
        await activityLogger.LogAsync(
            runId,
            AgentNames.Research,
            ActivityType.AgentCompleted,
            urls.Count == 0
                ? "No target URLs found in seed keywords; awaiting human approval to continue"
                : $"Proposed {urls.Count} target URL(s) for human approval before fetching",
            cancellationToken: cancellationToken);

        return urls;
    }

    public async Task<string> RunOutlineAsync(
        Guid runId,
        ArticleCategory category,
        string topic,
        string researchBrief,
        CancellationToken cancellationToken = default)
    {
        await LogStart(runId, AgentNames.Outline, "Building outline", cancellationToken);

        var memory = await memoryService.RetrieveContextAsync(
            AgentNames.Outline,
            category,
            topic,
            cancellationToken);

        var userPrompt = $"""
            Topic: {topic}
            Category: {category}

            Research brief:
            {researchBrief}

            Produce a Markdown outline with H2/H3 headings, learning objectives, and code example slots.
            """;

        var result = await llmClient.CompleteAsync(
            AppendMemory(AgentPrompts.OutlineSystem, memory),
            userPrompt,
            cancellationToken);

        await LogComplete(runId, AgentNames.Outline, "Outline created", cancellationToken);
        return result;
    }

    public async Task<string> RunWriterAsync(
        Guid runId,
        ArticleCategory category,
        string topic,
        string outline,
        string? revisionNotes,
        CancellationToken cancellationToken = default)
    {
        await LogStart(runId, AgentNames.Writer, "Writing draft", cancellationToken);

        var memory = await memoryService.RetrieveContextAsync(
            AgentNames.Writer,
            category,
            topic,
            cancellationToken);

        var userPrompt = $"""
            Topic: {topic}
            Category: {category}

            Outline:
            {outline}

            Revision notes:
            {revisionNotes ?? "None"}

            Write the full article in Markdown with fenced code blocks where appropriate.
            """;

        var result = await llmClient.CompleteAsync(
            AppendMemory(AgentPrompts.WriterSystem(category), memory),
            userPrompt,
            cancellationToken);

        await LogComplete(runId, AgentNames.Writer, "Draft written", cancellationToken);
        return result;
    }

    public async Task<string> RunTechnicalReviewAsync(
        Guid runId,
        ArticleCategory category,
        string topic,
        string draft,
        CancellationToken cancellationToken = default)
    {
        await LogStart(runId, AgentNames.TechnicalReviewer, "Technical review", cancellationToken);

        var memory = await memoryService.RetrieveContextAsync(
            AgentNames.TechnicalReviewer,
            category,
            topic,
            cancellationToken);

        var result = await llmClient.CompleteAsync(
            AppendMemory(AgentPrompts.TechnicalReviewerSystem, memory),
            $"Review this draft:\n\n{draft}",
            cancellationToken);

        await LogComplete(runId, AgentNames.TechnicalReviewer, "Technical review complete", cancellationToken);
        return result;
    }

    public async Task<string> RunEditorAsync(
        Guid runId,
        ArticleCategory category,
        string topic,
        string draft,
        string technicalReview,
        CancellationToken cancellationToken = default)
    {
        await LogStart(runId, AgentNames.Editor, "Editorial pass", cancellationToken);

        var memory = await memoryService.RetrieveContextAsync(
            AgentNames.Editor,
            category,
            topic,
            cancellationToken);

        var userPrompt = $"""
            Draft:
            {draft}

            Technical review notes:
            {technicalReview}

            Return the polished article in Markdown and list the main edits at the end.
            """;

        var result = await llmClient.CompleteAsync(
            AppendMemory(AgentPrompts.EditorSystem, memory),
            userPrompt,
            cancellationToken);

        await LogComplete(runId, AgentNames.Editor, "Editorial pass complete", cancellationToken);
        return result;
    }

    private static string AppendMemory(string systemPrompt, MemoryContext memory)
    {
        if (string.IsNullOrWhiteSpace(memory.FormattedBlock))
        {
            return systemPrompt;
        }

        return systemPrompt + "\n\n" + memory.FormattedBlock;
    }

    private Task LogStart(Guid runId, string agentName, string message, CancellationToken cancellationToken) =>
        activityLogger.LogAsync(
            runId,
            agentName,
            ActivityType.AgentStarted,
            message,
            cancellationToken: cancellationToken);

    private Task LogComplete(Guid runId, string agentName, string message, CancellationToken cancellationToken) =>
        activityLogger.LogAsync(
            runId,
            agentName,
            ActivityType.AgentCompleted,
            message,
            cancellationToken: cancellationToken);
}
