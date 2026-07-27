using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;

namespace Portfolio.Infrastructure.Persistence;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
    : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTranslation> ProjectTranslations => Set<ProjectTranslation>();
    public DbSet<ProjectMedia> ProjectMedia => Set<ProjectMedia>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.Slug).IsUnique();
            entity.Property(item => item.Slug).HasMaxLength(120).IsRequired();
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(item => item.PreviewUrl).HasMaxLength(500);
            entity.Property(item => item.RepositoryUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<ProjectTranslation>(entity =>
        {
            entity.HasKey(item => new { item.ProjectId, item.Culture });
            entity.Property(item => item.Culture).HasMaxLength(5).IsRequired();
            entity.Property(item => item.Title).HasMaxLength(180).IsRequired();
            entity.Property(item => item.Category).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Summary).HasMaxLength(900).IsRequired();
            entity.HasOne(item => item.Project)
                .WithMany(project => project.Translations)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMedia>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.FileName).HasMaxLength(300).IsRequired();
            entity.Property(item => item.AltTextEn).HasMaxLength(300);
            entity.Property(item => item.AltTextAr).HasMaxLength(300);
            entity.HasOne(item => item.Project)
                .WithMany(project => project.Media)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Technology>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.Slug).IsUnique();
            entity.Property(item => item.Name).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Slug).HasMaxLength(80).IsRequired();
            entity.Property(item => item.Category).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<ProjectTechnology>(entity =>
        {
            entity.HasKey(item => new { item.ProjectId, item.TechnologyId });
            entity.HasOne(item => item.Project)
                .WithMany(project => project.Technologies)
                .HasForeignKey(item => item.ProjectId);
            entity.HasOne(item => item.Technology)
                .WithMany(technology => technology.Projects)
                .HasForeignKey(item => item.TechnologyId);
        });
    }
}
