using System.Text.RegularExpressions;

namespace LightBooksAgent.Core.Utilities;

public static partial class ResearchUrlParser
{
    public static IReadOnlyList<string> ParseFromSeedKeywords(string seedKeywords) =>
        seedKeywords
            .Split([' ', '\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries)
            .Where(token => token.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                            token.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public static IReadOnlyList<string> ParseFromText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return UrlPattern()
            .Matches(text)
            .Select(match => match.Value.TrimEnd('.', ',', ';'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> ResolveApprovedUrls(
        IReadOnlyList<string> proposedUrls,
        bool approved,
        string? humanComment)
    {
        if (!approved)
        {
            return [];
        }

        var fromComment = ParseFromText(humanComment);
        if (fromComment.Count > 0)
        {
            return fromComment;
        }

        return proposedUrls.ToList();
    }

    public static string FormatForHumanReview(IReadOnlyList<string> urls)
    {
        if (urls.Count == 0)
        {
            return """
                   No HTTP/HTTPS URLs were found in seed keywords.
                   Approve to continue research without fetching external pages.
                   To fetch specific sites, add URLs to seed keywords and reject this review to re-propose.
                   """;
        }

        return string.Join(
            Environment.NewLine,
            urls.Select((url, index) => $"{index + 1}. {url}"));
    }

    [GeneratedRegex(@"https?://[^\s<>""']+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlPattern();
}
