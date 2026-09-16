using MDWeb.Application;
using MDWeb.Application.Building;
using MDWeb.Core.Abstractions;
using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Models;
using LightBooksAgent.Core.Options;
using MDWeb.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LightBooksAgent.Infrastructure.Publishing;

public sealed class MdWebPublisher(IOptions<MdWebOptions> options) : IMdWebPublisher
{
    public async Task<PublishResult> GenerateHtmlAsync(
        string sourceDirectory,
        string outputDirectory,
        string title,
        bool useWeChatTheme = false,
        CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var themeRelative = useWeChatTheme ? settings.WeChatTheme : settings.DefaultTheme;
        var themeDirectory = Path.Combine(settings.ProjectPath, themeRelative);

        if (!Directory.Exists(sourceDirectory))
        {
            return new PublishResult
            {
                Success = false,
                Errors = [$"Markdown source directory not found: {sourceDirectory}"]
            };
        }

        if (!Directory.Exists(themeDirectory))
        {
            return new PublishResult
            {
                Success = false,
                Errors = [$"MDWeb theme not found: {themeDirectory}"]
            };
        }

        Directory.CreateDirectory(outputDirectory);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMDWebApplication();
                services.AddMDWebInfrastructure(themeDirectory);
            })
            .Build();

        var builder = host.Services.GetRequiredService<ISiteBuilder>();
        var config = builder
            .WithSource(sourceDirectory)
            .WithOutput(outputDirectory)
            .WithTheme(themeDirectory)
            .WithTitle(title)
            .WithDescription($"Article published by LightBooksAgent: {title}")
            .Build();

        var generator = host.Services.GetRequiredService<ISiteGenerator>();
        var result = await generator.GenerateAsync(config, cancellationToken);

        if (!result.Success)
        {
            return new PublishResult
            {
                Success = false,
                Errors = result.Errors.ToList()
            };
        }

        return new PublishResult
        {
            Success = true,
            HtmlOutputPath = result.OutputDirectory
        };
    }
}
