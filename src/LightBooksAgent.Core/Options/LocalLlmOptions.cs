namespace LightBooksAgent.Core.Options;

public sealed class LocalLlmOptions
{
    public const string SectionName = "LocalLlm";

    public string BaseUrl { get; set; } = "http://localhost:9931/v1";

    public string Model { get; set; } = "local-model";

    public string EmbeddingModel { get; set; } = "local-model";

    public int MaxTokens { get; set; } = 4096;

    public float Temperature { get; set; } = 0.7f;
}
