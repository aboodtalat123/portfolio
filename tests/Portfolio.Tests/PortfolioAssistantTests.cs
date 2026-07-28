using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Repositories;
using Portfolio.Web.Assistant;

namespace Portfolio.Tests;

public sealed class PortfolioAssistantTests
{
    [Theory]
    [InlineData("https://example.com/work/dr-solution")]
    [InlineData("//example.com/work/dr-solution")]
    [InlineData("/work/video-goals-extractor")]
    [InlineData("/admin")]
    public void RequestValidator_RejectsExternalAndUnpublishedPaths(string path)
    {
        var request = new AssistantChatRequest
        {
            Question = "Tell me about this project",
            Path = path
        };

        var valid = AssistantRequestValidator.TryValidate(request, out _, out var error);

        Assert.False(valid);
        Assert.Equal("invalid_path", error);
    }

    [Fact]
    public void RequestValidator_NormalizesLanguageSectionAndHistory()
    {
        var request = new AssistantChatRequest
        {
            Question = "  What   does he build?  ",
            Language = "AR",
            Path = "/work/dr-solution?lang=ar",
            Section = "#unknown-section",
            PageContext = new string('x', AssistantRequestValidator.MaximumPageContextLength + 20),
            History = Enumerable.Range(1, 9)
                .Select(index => new AssistantConversationTurn
                {
                    Role = index % 2 == 0 ? "assistant" : "anything-else",
                    Content = $" Turn   {index} "
                })
                .ToArray()
        };

        var valid = AssistantRequestValidator.TryValidate(request, out var normalized, out _);

        Assert.True(valid);
        Assert.NotNull(normalized);
        Assert.Equal("What does he build?", normalized.Question);
        Assert.Equal("ar", normalized.Language);
        Assert.Equal("/work/dr-solution", normalized.Path);
        Assert.Equal(string.Empty, normalized.Section);
        Assert.Equal(AssistantRequestValidator.MaximumPageContextLength, normalized.PageContext.Length);
        Assert.Equal(AssistantRequestValidator.MaximumHistoryTurns, normalized.History.Count);
        Assert.All(
            normalized.History,
            turn => Assert.Contains(turn.Role, new[] { "user", "assistant" }));
    }

    [Fact]
    public async Task KnowledgeService_PrioritizesCurrentArabicProjectAndExcludesVideoLab()
    {
        await using var fixture = await AssistantDatabaseFixture.CreateAsync();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new PortfolioKnowledgeService(fixture.Repository, cache);

        var context = await service.GetContextAsync(
            "اشرح المشروع الموجود في هذه الصفحة",
            "ar",
            "/work/dr-solution",
            string.Empty);

        Assert.Equal("ar", context.Language);
        Assert.Equal("dr-solution", context.Documents[0].SourceId);
        Assert.Contains("?lang=ar", context.Documents[0].Url);
        Assert.DoesNotContain(
            context.Documents,
            document => document.SourceId.Contains("video", StringComparison.OrdinalIgnoreCase)
                || document.Content.Contains("Video Goals Extractor", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task KnowledgeService_WorkSectionUsesOnlyThreeSelectedProjects()
    {
        await using var fixture = await AssistantDatabaseFixture.CreateAsync();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new PortfolioKnowledgeService(fixture.Repository, cache);

        var context = await service.GetContextAsync(
            "What are the main projects?",
            "en",
            "/",
            "work");

        var sourceIds = context.Documents.Select(document => document.SourceId).ToHashSet();
        Assert.Contains("site-ai", sourceIds);
        Assert.Contains("dr-solution", sourceIds);
        Assert.Contains("fairaid", sourceIds);
        Assert.DoesNotContain("video-goals-extractor", sourceIds);
    }

    private sealed class AssistantDatabaseFixture(
        SqliteConnection connection,
        PortfolioDbContext dbContext) : IAsyncDisposable
    {
        public ProjectRepository Repository { get; } = new(dbContext);

        public static async Task<AssistantDatabaseFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<PortfolioDbContext>()
                .UseSqlite(connection)
                .Options;
            var dbContext = new PortfolioDbContext(options);
            await PortfolioDatabase.InitializeAsync(dbContext);
            return new AssistantDatabaseFixture(connection, dbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await dbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
