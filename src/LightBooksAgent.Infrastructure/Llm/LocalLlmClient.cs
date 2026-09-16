using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Models;
using LightBooksAgent.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LightBooksAgent.Infrastructure.Llm;

public sealed class LocalLlmClient(
    HttpClient httpClient,
    IOptions<LocalLlmOptions> options,
    ILogger<LocalLlmClient> logger) : ILocalLlmClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var requestBody = new ChatCompletionRequest
        {
            Model = settings.Model,
            Temperature = settings.Temperature,
            MaxTokens = settings.MaxTokens,
            Messages =
            [
                new ChatMessage { Role = "system", Content = systemPrompt },
                new ChatMessage { Role = "user", Content = userPrompt }
            ]
        };

        var stopwatch = Stopwatch.StartNew();
        using var response = await httpClient.PostAsJsonAsync(
            "chat/completions",
            requestBody,
            SerializerOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(
            SerializerOptions,
            cancellationToken);

        stopwatch.Stop();
        logger.LogDebug(
            "LLM completion finished in {ElapsedMs}ms",
            stopwatch.ElapsedMilliseconds);

        return payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim()
            ?? string.Empty;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var requestBody = new EmbeddingRequest
        {
            Model = settings.EmbeddingModel,
            Input = text
        };

        using var response = await httpClient.PostAsJsonAsync(
            "embeddings",
            requestBody,
            SerializerOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(
            SerializerOptions,
            cancellationToken);

        var vector = payload?.Data?.FirstOrDefault()?.Embedding;
        return vector ?? [];
    }

    public async Task<LlmHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync("models", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new LlmHealthStatus
                {
                    IsHealthy = false,
                    Message = $"LLM service returned {(int)response.StatusCode}"
                };
            }

            var payload = await response.Content.ReadFromJsonAsync<ModelsResponse>(
                SerializerOptions,
                cancellationToken);

            var modelName = payload?.Data?.FirstOrDefault()?.Id ?? options.Value.Model;
            return new LlmHealthStatus
            {
                IsHealthy = true,
                ModelName = modelName,
                Message = "Local LLM is reachable"
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Local LLM health check failed");
            return new LlmHealthStatus
            {
                IsHealthy = false,
                Message = ex.Message
            };
        }
    }

    private sealed class ChatCompletionRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<ChatMessage> Messages { get; set; } = [];
        public float Temperature { get; set; }
        public int MaxTokens { get; set; }
    }

    private sealed class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private sealed class ChatCompletionResponse
    {
        public List<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        public ChatMessage? Message { get; set; }
    }

    private sealed class EmbeddingRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;
    }

    private sealed class EmbeddingResponse
    {
        public List<EmbeddingData>? Data { get; set; }
    }

    private sealed class EmbeddingData
    {
        public float[] Embedding { get; set; } = [];
    }

    private sealed class ModelsResponse
    {
        public List<ModelInfo>? Data { get; set; }
    }

    private sealed class ModelInfo
    {
        public string Id { get; set; } = string.Empty;
    }
}
