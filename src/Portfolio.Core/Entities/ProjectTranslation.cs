namespace Portfolio.Core.Entities;

public sealed class ProjectTranslation
{
    public Guid ProjectId { get; set; }
    public string Culture { get; set; } = "en";
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Challenge { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
}
