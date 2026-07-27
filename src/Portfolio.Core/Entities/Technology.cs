namespace Portfolio.Core.Entities;

public sealed class Technology
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public ICollection<ProjectTechnology> Projects { get; set; } = [];
}
