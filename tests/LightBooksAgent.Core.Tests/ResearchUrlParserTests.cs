using LightBooksAgent.Core.Utilities;

namespace LightBooksAgent.Core.Tests;

public class ResearchUrlParserTests
{
    [Fact]
    public void ParseFromSeedKeywords_ExtractsHttpUrls()
    {
        var urls = ResearchUrlParser.ParseFromSeedKeywords(
            "csharp async https://example.com/docs http://localhost:8080 notes");

        Assert.Equal(2, urls.Count);
        Assert.Contains("https://example.com/docs", urls);
        Assert.Contains("http://localhost:8080", urls);
    }

    [Fact]
    public void ResolveApprovedUrls_OnReject_ReturnsEmpty()
    {
        var result = ResearchUrlParser.ResolveApprovedUrls(
            ["https://example.com"],
            approved: false,
            humanComment: null);

        Assert.Empty(result);
    }

    [Fact]
    public void ResolveApprovedUrls_OnApprove_UsesProposedWhenCommentEmpty()
    {
        var proposed = new[] { "https://a.test", "https://b.test" };
        var result = ResearchUrlParser.ResolveApprovedUrls(proposed, true, null);

        Assert.Equal(proposed, result);
    }

    [Fact]
    public void ResolveApprovedUrls_OnApprove_UsesUrlsFromCommentWhenPresent()
    {
        var result = ResearchUrlParser.ResolveApprovedUrls(
            ["https://a.test", "https://b.test"],
            true,
            "Please only use https://b.test and https://c.test");

        Assert.Equal(2, result.Count);
        Assert.Contains("https://b.test", result);
        Assert.Contains("https://c.test", result);
    }

    [Fact]
    public void FormatForHumanReview_ListsUrls()
    {
        var text = ResearchUrlParser.FormatForHumanReview(["https://example.com"]);
        Assert.Contains("1. https://example.com", text);
    }
}
