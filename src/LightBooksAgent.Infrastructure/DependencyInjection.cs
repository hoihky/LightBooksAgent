using LightBooksAgent.Core.Interfaces;
using LightBooksAgent.Core.Options;
using LightBooksAgent.Infrastructure.Llm;
using LightBooksAgent.Infrastructure.Memory;
using LightBooksAgent.Infrastructure.Publishing;
using LightBooksAgent.Infrastructure.Url;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LightBooksAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLightBooksInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LocalLlmOptions>(configuration.GetSection(LocalLlmOptions.SectionName));
        services.Configure<MdWebOptions>(configuration.GetSection(MdWebOptions.SectionName));
        services.Configure<PublishingOptions>(configuration.GetSection(PublishingOptions.SectionName));

        var llmOptions = configuration.GetSection(LocalLlmOptions.SectionName).Get<LocalLlmOptions>()
            ?? new LocalLlmOptions();

        services.AddHttpClient<ILocalLlmClient, LocalLlmClient>(client =>
        {
            client.BaseAddress = new Uri(llmOptions.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromMinutes(10);
        });

        var publishingOptions = configuration.GetSection(PublishingOptions.SectionName).Get<PublishingOptions>()
            ?? new PublishingOptions();

        services.AddHttpClient<IUrlFetcher, UrlFetcher>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(publishingOptions.UrlFetchTimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("LightBooksAgent/1.0");
        });

        services.AddScoped<IMemoryService, MemoryService>();
        services.AddScoped<IMdWebPublisher, MdWebPublisher>();
        services.AddScoped<IPublishingService, PublishingService>();

        return services;
    }
}
