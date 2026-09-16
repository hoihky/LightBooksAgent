using LightBooksAgent.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LightBooksAgent.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        var connectionString = db.Database.GetConnectionString() ?? string.Empty;
        var dataDirectory = ExtractDataDirectory(connectionString);
        if (!string.IsNullOrWhiteSpace(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialized.");
    }

    private static string? ExtractDataDirectory(string connectionString)
    {
        const string prefix = "Data Source=";
        if (!connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var path = connectionString[prefix.Length..].Trim();
        return Path.GetDirectoryName(Path.GetFullPath(path));
    }
}
