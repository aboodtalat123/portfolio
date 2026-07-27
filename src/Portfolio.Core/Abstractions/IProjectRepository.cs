using Portfolio.Core.Models;

namespace Portfolio.Core.Abstractions;

public interface IProjectRepository
{
    Task<IReadOnlyList<ProjectCard>> GetPublishedAsync(
        string culture = "en",
        CancellationToken cancellationToken = default);

    Task<ProjectDetail?> GetBySlugAsync(
        string slug,
        string culture = "en",
        CancellationToken cancellationToken = default);
}
