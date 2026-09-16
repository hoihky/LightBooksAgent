using LightBooksAgent.Application.Persistence;
using LightBooksAgent.Application.Services;
using LightBooksAgent.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LightBooksAgent.Core.Options;

namespace LightBooksAgent.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddLightBooksApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? new DatabaseOptions();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(dbOptions.ConnectionString));

        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IHitlService, HitlService>();
        services.AddScoped<IActivityLogger, ActivityService>();

        return services;
    }
}
