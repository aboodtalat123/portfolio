namespace Portfolio.Core.Entities;

public sealed class ProjectMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string AltTextEn { get; set; } = string.Empty;
    public string AltTextAr { get; set; } = string.Empty;
    public bool IsThumbnail { get; set; }
    public int SortOrder { get; set; }

    public Project Project { get; set; } = null!;
}
