namespace Portfolio.Web.Assistant;

public sealed class AssistantChatRequest
{
    public string? Question { get; init; }

    public string? Language { get; init; }

    public string? Path { get; init; }

    public string? Section { get; init; }

    public string? PageContext { get; init; }

    public IReadOnlyList<AssistantConversationTurn>? History { get; init; }
}

public sealed class AssistantConversationTurn
{
    public string? Role { get; init; }

    public string? Content { get; init; }
}

public sealed record AssistantSource(
    string Id,
    string Title,
    string Url);

public sealed record AssistantChatResponse(
    string Answer,
    IReadOnlyList<AssistantSource> Sources,
    string Confidence, AssistantNavigation? Navigation = null);

public sealed record AssistantKnowledgeDocument(
    string SourceId,
    string Title,
    string Url,
    string Content,
    IReadOnlyList<string> Keywords);

public sealed record AssistantKnowledgeContext(
    string Language,
    IReadOnlyList<AssistantKnowledgeDocument> Documents);

public sealed record ValidatedAssistantRequest(
    string Question,
    string Language,
    string Path,
    string Section,
    string PageContext,
    IReadOnlyList<AssistantConversationTurn> History);

public sealed record PortfolioAssistantResult(
    string Answer,
    IReadOnlyList<AssistantSource> Sources,
    string Confidence);

public sealed class PortfolioAssistantOptions
{
    public const string DefaultModel = "gemini-3.1-flash-lite";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = DefaultModel;

    public int DailyRequestLimit { get; set; } = 150;
}

public interface IPortfolioKnowledgeService
{
    Task<AssistantKnowledgeContext> GetContextAsync(
        string question,
        string language,
        string path,
        string section,
        CancellationToken cancellationToken = default);
}

public interface IPortfolioAssistantService
{
    Task<PortfolioAssistantResult> AskAsync(
        ValidatedAssistantRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class PortfolioAssistantUnavailableException(string message, Exception? innerException = null)
    : Exception(message, innerException);
