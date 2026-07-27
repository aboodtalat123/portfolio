namespace Portfolio.Core.Entities;

public sealed class ProjectTechnology
{
    public Guid ProjectId { get; set; }
    public Guid TechnologyId { get; set; }

    public Project Project { get; set; } = null!;
    public Technology Technology { get; set; } = null!;
}
