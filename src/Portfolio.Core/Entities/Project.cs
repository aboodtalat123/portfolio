namespace Portfolio.Core.Entities;

public sealed class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Slug { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public int CompletedYear { get; set; }
    public string? PreviewUrl { get; set; }
    public string? RepositoryUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ProjectTranslation> Translations { get; set; } = [];
    public ICollection<ProjectMedia> Media { get; set; } = [];
    public ICollection<ProjectTechnology> Technologies { get; set; } = [];
}
