using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Repositories;

namespace Portfolio.Tests;

public sealed class PortfolioDatabaseTests
{
    [Fact]
    public async Task InitializeAsync_SeedsFourBilingualProjects_OnlyOnce()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new PortfolioDbContext(options);
        await PortfolioDatabase.InitializeAsync(dbContext);
        await PortfolioDatabase.InitializeAsync(dbContext);

        var repository = new ProjectRepository(dbContext);
        var englishProjects = await repository.GetPublishedAsync("en");
        var arabicProjects = await repository.GetPublishedAsync("ar");

        Assert.Equal(4, englishProjects.Count);
        Assert.Equal(4, arabicProjects.Count);
        Assert.Equal("Safe Site-Aware AI Layer", englishProjects[0].Title);
        Assert.Equal("طبقة ذكاء آمنة ومدركة للموقع", arabicProjects[0].Title);
        Assert.Contains(englishProjects, project => project.Slug == "reusable-site-ai");
        Assert.Contains(englishProjects, project => project.Slug == "dr-solution");
        Assert.All(englishProjects, project => Assert.NotEmpty(project.Technologies));
        Assert.All(englishProjects, project => Assert.False(string.IsNullOrWhiteSpace(project.MediaPath)));
        Assert.Null(englishProjects.Single(project => project.Slug == "reusable-site-ai").PreviewUrl);

        var drSolution = await repository.GetBySlugAsync("dr-solution", "en");
        Assert.NotNull(drSolution);
        Assert.Equal("https://dr-solution.onrender.com", drSolution.PreviewUrl);
        Assert.Contains("BYOK", drSolution.Outcome);
        Assert.Contains("PyMuPDF", drSolution.Technologies);
    }

    [Fact]
    public async Task InitializeAsync_RestoresMissingProjectMedia_ForExistingDatabase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new PortfolioDbContext(options);
        await PortfolioDatabase.InitializeAsync(dbContext);

        dbContext.ProjectMedia.RemoveRange(dbContext.ProjectMedia);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        await PortfolioDatabase.InitializeAsync(dbContext);

        Assert.Equal(4, await dbContext.ProjectMedia.CountAsync());
    }
}
