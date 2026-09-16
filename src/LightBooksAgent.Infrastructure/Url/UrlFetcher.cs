using System.Net;
using System.Text.RegularExpressions;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LightBooksAgent.Infrastructure.Url;

public sealed partial class UrlFetcher(
    HttpClient httpClient,
    IOptions<PublishingOptions> options,
    ILogger<UrlFetcher> logger) : IUrlFetcher
{
    public async Task<(bool Success, string Title, string Content, string? Error)> FetchAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (!IsUrlAllowed(url))
        {
            return (false, string.Empty, string.Empty, "URL is not in the allowlist.");
        }

        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, string.Empty, string.Empty, $"HTTP {(int)response.StatusCode}");
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var title = ExtractTitle(html) ?? url;
            var text = StripHtml(html);

            if (text.Length > 12000)
            {
                text = text[..12000];
            }

            return (true, title, text, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch URL {Url}", url);
            return (false, string.Empty, string.Empty, ex.Message);
        }
    }

    private bool IsUrlAllowed(string url)
    {
        var allowlist = options.Value.UrlAllowlist;
        if (allowlist.Length == 0)
        {
            return true;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return allowlist.Any(entry =>
            uri.Host.Equals(entry, StringComparison.OrdinalIgnoreCase) ||
            uri.Host.EndsWith("." + entry, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractTitle(string html)
    {
        var match = TitleTagRegex().Match(html);
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value.Trim()) : null;
    }

    private static string StripHtml(string html)
    {
        var withoutScripts = ScriptTagRegex().Replace(html, " ");
        var withoutStyles = StyleTagRegex().Replace(withoutScripts, " ");
        var withoutTags = HtmlTagRegex().Replace(withoutStyles, " ");
        return WebUtility.HtmlDecode(withoutTags).Replace('\n', ' ').Trim();
    }

    [GeneratedRegex("<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TitleTagRegex();

    [GeneratedRegex("<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex("<style[^>]*>.*?</style>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex StyleTagRegex();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTagRegex();
}
