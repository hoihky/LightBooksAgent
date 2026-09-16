using LightBooksAgent.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LightBooksAgent.Application.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ArticleProject> ArticleProjects => Set<ArticleProject>();
    public DbSet<PublishingRun> PublishingRuns => Set<PublishingRun>();
    public DbSet<AgentActivity> AgentActivities => Set<AgentActivity>();
    public DbSet<ResearchMaterial> ResearchMaterials => Set<ResearchMaterial>();
    public DbSet<ArticleVersion> ArticleVersions => Set<ArticleVersion>();
    public DbSet<ReviewRequest> ReviewRequests => Set<ReviewRequest>();
    public DbSet<AgentMemoryEntry> AgentMemoryEntries => Set<AgentMemoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureDateTimeOffsetConverters(modelBuilder);

        modelBuilder.Entity<ArticleProject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Audience).HasMaxLength(200);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<PublishingRun>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ArticleProject)
                .WithMany(p => p.Runs)
                .HasForeignKey(e => e.ArticleProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.AgentStatus);
        });

        modelBuilder.Entity<AgentActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.PublishingRun)
                .WithMany(r => r.Activities)
                .HasForeignKey(e => e.PublishingRunId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<ResearchMaterial>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).HasMaxLength(2000);
            entity.HasOne(e => e.PublishingRun)
                .WithMany(r => r.ResearchMaterials)
                .HasForeignKey(e => e.PublishingRunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ArticleVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ArticleProject)
                .WithMany(p => p.Versions)
                .HasForeignKey(e => e.ArticleProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ArticleProjectId, e.VersionNumber }).IsUnique();
        });

        modelBuilder.Entity<ReviewRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.PublishingRun)
                .WithMany(r => r.ReviewRequests)
                .HasForeignKey(e => e.PublishingRunId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<AgentMemoryEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.ArticleProject)
                .WithMany(p => p.Memories)
                .HasForeignKey(e => e.ArticleProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.AgentName, e.Layer });
        });
    }

    private static void ConfigureDateTimeOffsetConverters(ModelBuilder modelBuilder)
    {
        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, long>(
            value => value.UtcTicks,
            value => new DateTimeOffset(value, TimeSpan.Zero));

        var nullableDateTimeOffsetConverter = new ValueConverter<DateTimeOffset?, long?>(
            value => value.HasValue ? value.Value.UtcTicks : null,
            value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(dateTimeOffsetConverter);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(nullableDateTimeOffsetConverter);
                }
            }
        }
    }
}
