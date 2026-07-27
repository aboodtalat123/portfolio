namespace Portfolio.Core.Models;

public sealed record ProjectCard(
    Guid Id,
    string Slug,
    string Title,
    string Category,
    string Summary,
    int CompletedYear,
    bool IsFeatured,
    string? PreviewUrl,
    string? RepositoryUrl,
    string? MediaPath,
    string? MediaAlt,
    IReadOnlyList<string> Technologies);
