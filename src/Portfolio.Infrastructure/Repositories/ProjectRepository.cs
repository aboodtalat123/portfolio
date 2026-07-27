using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Abstractions;
using Portfolio.Core.Entities;
using Portfolio.Core.Models;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Repositories;

public sealed class ProjectRepository(PortfolioDbContext dbContext) : IProjectRepository
{
    public async Task<IReadOnlyList<ProjectCard>> GetPublishedAsync(
        string culture = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedCulture = string.Equals(culture, "ar", StringComparison.OrdinalIgnoreCase)
            ? "ar"
            : "en";

        var projects = await dbContext.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Where(project => project.Status == ProjectStatus.Published)
            .Include(project => project.Translations)
            .Include(project => project.Media)
            .Include(project => project.Technologies)
                .ThenInclude(link => link.Technology)
            .OrderByDescending(project => project.IsFeatured)
            .ThenBy(project => project.SortOrder)
            .ToListAsync(cancellationToken);

        return projects.Select(project =>
        {
            var translation = project.Translations.FirstOrDefault(item => item.Culture == normalizedCulture)
                ?? project.Translations.First(item => item.Culture == "en");
            var media = project.Media
                .OrderByDescending(item => item.IsThumbnail)
                .ThenBy(item => item.SortOrder)
                .FirstOrDefault();

            return new ProjectCard(
                project.Id,
                project.Slug,
                translation.Title,
                translation.Category,
                translation.Summary,
                project.CompletedYear,
                project.IsFeatured,
                project.PreviewUrl,
                project.RepositoryUrl,
                media?.FileName,
                media is null
                    ? null
                    : normalizedCulture == "ar" ? media.AltTextAr : media.AltTextEn,
                project.Technologies
                    .OrderBy(link => link.Technology.Name)
                    .Select(link => link.Technology.Name)
                    .ToArray());
        }).ToArray();
    }

    public async Task<ProjectDetail?> GetBySlugAsync(
        string slug,
        string culture = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var normalizedCulture = string.Equals(culture, "ar", StringComparison.OrdinalIgnoreCase)
            ? "ar"
            : "en";

        var project = await dbContext.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Where(item => item.Status == ProjectStatus.Published && item.Slug == normalizedSlug)
            .Include(item => item.Translations)
            .Include(item => item.Media)
            .Include(item => item.Technologies)
                .ThenInclude(link => link.Technology)
            .SingleOrDefaultAsync(cancellationToken);

        if (project is null)
        {
            return null;
        }

        var translation = project.Translations.FirstOrDefault(item => item.Culture == normalizedCulture)
            ?? project.Translations.First(item => item.Culture == "en");

        return new ProjectDetail(
            project.Id,
            project.Slug,
            translation.Title,
            translation.Category,
            translation.Summary,
            translation.Challenge,
            translation.Solution,
            translation.Role,
            translation.Outcome,
            project.CompletedYear,
            project.IsFeatured,
            project.PreviewUrl,
            project.RepositoryUrl,
            project.Media
                .OrderByDescending(item => item.IsThumbnail)
                .ThenBy(item => item.SortOrder)
                .Select(item => new ProjectMediaItem(
                    item.FileName,
                    normalizedCulture == "ar" ? item.AltTextAr : item.AltTextEn,
                    item.IsThumbnail,
                    item.SortOrder))
                .ToArray(),
            project.Technologies
                .OrderBy(link => link.Technology.Name)
                .Select(link => link.Technology.Name)
                .ToArray());
    }
}
