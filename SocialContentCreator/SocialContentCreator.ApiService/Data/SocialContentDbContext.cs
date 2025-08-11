using Microsoft.EntityFrameworkCore;
using SocialContentCreator.ApiService.Models;

namespace SocialContentCreator.ApiService.Data;

public class SocialContentDbContext : DbContext
{
    public SocialContentDbContext(DbContextOptions<SocialContentDbContext> options) : base(options)
    {
    }

    public DbSet<Content> Contents { get; set; }
    public DbSet<SocialMediaPost> SocialMediaPosts { get; set; }
    public DbSet<SocialMediaPostMetrics> SocialMediaPostMetrics { get; set; }
    public DbSet<ContentTag> ContentTags { get; set; }
    public DbSet<BrandVoice> BrandVoices { get; set; }
    public DbSet<ContentCalendar> ContentCalendars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Content configuration
        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OriginalContent).IsRequired();
            entity.Property(e => e.Source).HasConversion<string>();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // SocialMediaPost configuration
        modelBuilder.Entity<SocialMediaPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
            entity.Property(e => e.GeneratedContent).IsRequired();
            entity.HasOne(e => e.Content)
                  .WithMany(e => e.SocialMediaPosts)
                  .HasForeignKey(e => e.ContentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Platform);
            entity.HasIndex(e => e.ScheduledAt);
        });

        // SocialMediaPostMetrics configuration
        modelBuilder.Entity<SocialMediaPostMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.SocialMediaPost)
                  .WithOne(e => e.Metrics)
                  .HasForeignKey<SocialMediaPostMetrics>(e => e.SocialMediaPostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ContentTag configuration
        modelBuilder.Entity<ContentTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Many-to-many relationship between Content and ContentTag
        modelBuilder.Entity<Content>()
            .HasMany(e => e.Tags)
            .WithMany(e => e.Contents)
            .UsingEntity(j => j.ToTable("ContentTagMappings"));

        // BrandVoice configuration
        modelBuilder.Entity<BrandVoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // ContentCalendar configuration
        modelBuilder.Entity<ContentCalendar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.SocialMediaPost)
                  .WithMany()
                  .HasForeignKey(e => e.SocialMediaPostId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ScheduledDateTime);
        });
    }
}
