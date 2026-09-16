namespace LightBooksAgent.Core.Interfaces;

public interface IUrlFetcher
{
    Task<(bool Success, string Title, string Content, string? Error)> FetchAsync(
        string url,
        CancellationToken cancellationToken = default);
}
