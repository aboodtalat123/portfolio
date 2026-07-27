namespace Portfolio.Core.Models;

public sealed record ProjectMediaItem(
    string Path,
    string? AltText,
    bool IsThumbnail,
    int SortOrder);

public sealed record ProjectDetail(
    Guid Id,
    string Slug,
    string Title,
    string Category,
    string Summary,
    string Challenge,
    string Solution,
    string Role,
    string Outcome,
    int CompletedYear,
    bool IsFeatured,
    string? PreviewUrl,
    string? RepositoryUrl,
    IReadOnlyList<ProjectMediaItem> Media,
    IReadOnlyList<string> Technologies);
