namespace LightBooksAgent.Core.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string ConnectionString { get; set; } = "Data Source=./data/lightbooks.db";
}
