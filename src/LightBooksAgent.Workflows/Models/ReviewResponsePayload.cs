namespace LightBooksAgent.Workflows.Models;

public sealed record ReviewResponsePayload(bool Approved, string? Comment);
