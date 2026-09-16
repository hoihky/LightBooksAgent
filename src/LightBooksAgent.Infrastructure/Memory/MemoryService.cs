using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Core.Enums;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Models;
using LightBooksAgent.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace LightBooksAgent.Infrastructure.Memory;

public sealed class MemoryService(AppDbContext db, ILocalLlmClient llmClient) : IMemoryService
{
    private const int MaxSemanticResults = 5;
    private const int MaxProceduralResults = 3;

    public async Task<MemoryContext> RetrieveContextAsync(
        string agentName,
        ArticleCategory? category,
        string topic,
        CancellationToken cancellationToken = default)
    {
        var lessons = new List<string>();

        var procedural = await db.AgentMemoryEntries
            .Where(m => m.Layer == MemoryLayer.Procedural && m.AgentName == agentName)
            .Where(m => category == null || m.Category == category)
            .OrderByDescending(m => m.Importance)
            .Take(MaxProceduralResults)
            .Select(m => m.Content)
            .ToListAsync(cancellationToken);

        lessons.AddRange(procedural);

        var topicEmbedding = await llmClient.EmbedAsync(topic, cancellationToken);
        if (topicEmbedding.Length > 0)
        {
            var semanticCandidates = await db.AgentMemoryEntries
                .Where(m => m.Layer == MemoryLayer.Semantic)
                .Where(m => m.Embedding != null)
                .Where(m => category == null || m.Category == category)
                .ToListAsync(cancellationToken);

            var ranked = semanticCandidates
                .Select(m => new
                {
                    Entry = m,
                    Score = CosineSimilarity(topicEmbedding, BytesToFloats(m.Embedding!))
                })
                .Where(x => x.Score > 0.3f)
                .OrderByDescending(x => x.Score * x.Entry.Importance)
                .Take(MaxSemanticResults)
                .Select(x => x.Entry.Content);

            lessons.AddRange(ranked);
        }

        return new MemoryContext { Lessons = lessons.Distinct().ToList() };
    }

    public async Task StoreEpisodicAsync(
        Guid articleProjectId,
        string agentName,
        string content,
        CancellationToken cancellationToken = default)
    {
        db.AgentMemoryEntries.Add(new AgentMemoryEntry
        {
            ArticleProjectId = articleProjectId,
            AgentName = agentName,
            Layer = MemoryLayer.Episodic,
            Content = content
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task StoreSemanticAsync(
        string agentName,
        ArticleCategory? category,
        string content,
        float importance = 0.5f,
        CancellationToken cancellationToken = default)
    {
        var embedding = await llmClient.EmbedAsync(content, cancellationToken);

        db.AgentMemoryEntries.Add(new AgentMemoryEntry
        {
            AgentName = agentName,
            Layer = MemoryLayer.Semantic,
            Category = category,
            Content = content,
            Importance = importance,
            Embedding = FloatsToBytes(embedding)
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DistillFromFeedbackAsync(
        Guid articleProjectId,
        string feedback,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(feedback))
        {
            return;
        }

        var project = await db.ArticleProjects.FindAsync([articleProjectId], cancellationToken);
        var category = project?.Category;

        await StoreEpisodicAsync(
            articleProjectId,
            "HumanReviewer",
            $"Feedback: {feedback.Trim()}",
            cancellationToken);

        var distilledPrompt =
            "Extract one concise lesson for future technical writing. " +
            $"Category: {category}. Feedback: {feedback}";

        var lesson = await llmClient.CompleteAsync(
            "You distill editorial feedback into short reusable writing lessons.",
            distilledPrompt,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(lesson))
        {
            await StoreSemanticAsync(
                "EditorAgent",
                category,
                lesson.Trim(),
                importance: 0.8f,
                cancellationToken: cancellationToken);
        }
    }

    public async Task<IReadOnlyList<string>> GetRecentMemoriesAsync(
        string? agentName = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = db.AgentMemoryEntries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(agentName))
        {
            query = query.Where(m => m.AgentName == agentName);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Select(m => $"[{m.Layer}/{m.AgentName}] {m.Content}")
            .ToListAsync(cancellationToken);
    }

    private static float CosineSimilarity(float[] left, float[] right)
    {
        if (left.Length == 0 || right.Length == 0 || left.Length != right.Length)
        {
            return 0f;
        }

        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;

        for (var i = 0; i < left.Length; i++)
        {
            dot += left[i] * right[i];
            leftNorm += left[i] * left[i];
            rightNorm += right[i] * right[i];
        }

        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0f;
        }

        return (float)(dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm)));
    }

    private static byte[] FloatsToBytes(float[] values)
    {
        var bytes = new byte[values.Length * sizeof(float)];
        Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private static float[] BytesToFloats(byte[] bytes)
    {
        var values = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);
        return values;
    }
}
