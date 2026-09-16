namespace LightBooksAgent.Core.Options;

public sealed class PublishingOptions
{
    public const string SectionName = "Publishing";

    public int MaxRevisionLoops { get; set; } = 3;

    public string ArticlesPath { get; set; } = "./articles";

    public int UrlFetchTimeoutSeconds { get; set; } = 30;

    public string[] UrlAllowlist { get; set; } = [];
}
