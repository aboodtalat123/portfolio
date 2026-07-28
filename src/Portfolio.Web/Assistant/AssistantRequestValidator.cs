using System.Text.RegularExpressions;

namespace Portfolio.Web.Assistant;

public static partial class AssistantRequestValidator
{
    public const int MaximumQuestionLength = 500;
    public const int MaximumHistoryTurns = 6;
    public const int MaximumHistoryTurnLength = 700;
    public const int MaximumPageContextLength = 5000;

    private static readonly HashSet<string> AllowedSections = new(StringComparer.OrdinalIgnoreCase)
    {
        string.Empty,
        "top",
        "system",
        "work",
        "about",
        "contact"
    };

    private static readonly HashSet<string> AllowedProjectSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "reusable-site-ai",
        "dr-solution",
        "fairaid-gaza"
    };

    public static bool TryValidate(
        AssistantChatRequest? request,
        out ValidatedAssistantRequest? validated,
        out string error)
    {
        validated = null;
        error = string.Empty;

        var question = CollapseWhitespace(request?.Question);
        if (question.Length < 2)
        {
            error = "question_required";
            return false;
        }

        if (question.Length > MaximumQuestionLength)
        {
            error = "question_too_long";
            return false;
        }

        if (!TryNormalizePath(request?.Path, out var path))
        {
            error = "invalid_path";
            return false;
        }

        var section = (request?.Section ?? string.Empty).Trim().TrimStart('#').ToLowerInvariant();
        if (!AllowedSections.Contains(section))
        {
            section = string.Empty;
        }

        var language = string.Equals(request?.Language, "ar", StringComparison.OrdinalIgnoreCase)
            ? "ar"
            : "en";
        var pageContext = CollapseWhitespace(request?.PageContext);
        pageContext = pageContext[..Math.Min(pageContext.Length, MaximumPageContextLength)];

        var history = (request?.History ?? [])
            .Where(turn => turn is not null)
            .Select(turn => new AssistantConversationTurn
            {
                Role = string.Equals(turn.Role, "assistant", StringComparison.OrdinalIgnoreCase)
                    ? "assistant"
                    : "user",
                Content = CollapseWhitespace(turn.Content)
            })
            .Where(turn => !string.IsNullOrWhiteSpace(turn.Content))
            .TakeLast(MaximumHistoryTurns)
            .Select(turn =>
            {
                var content = turn.Content ?? string.Empty;
                return new AssistantConversationTurn
                {
                    Role = turn.Role,
                    Content = content[..Math.Min(content.Length, MaximumHistoryTurnLength)]
                };
            })
            .ToArray();

        validated = new ValidatedAssistantRequest(
            question,
            language,
            path,
            section,
            pageContext,
            history);
        return true;
    }

    public static bool TryNormalizePath(string? value, out string path)
    {
        path = "/";
        var candidate = string.IsNullOrWhiteSpace(value) ? "/" : value.Trim();

        if (candidate.Length > 180
            || !candidate.StartsWith("/", StringComparison.Ordinal)
            || candidate.StartsWith("//", StringComparison.Ordinal)
            || candidate.Contains('\\')
            || candidate.Contains("://", StringComparison.Ordinal))
        {
            return false;
        }

        var pathOnly = candidate.Split('?', '#')[0].TrimEnd('/');
        if (string.IsNullOrEmpty(pathOnly))
        {
            path = "/";
            return true;
        }

        if (string.Equals(pathOnly, "/", StringComparison.Ordinal))
        {
            path = "/";
            return true;
        }

        var match = ProjectPathRegex().Match(pathOnly);
        if (!match.Success || !AllowedProjectSlugs.Contains(match.Groups["slug"].Value))
        {
            return false;
        }

        path = $"/work/{match.Groups["slug"].Value.ToLowerInvariant()}";
        return true;
    }

    private static string CollapseWhitespace(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : WhitespaceRegex().Replace(value.Trim(), " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"^/work/(?<slug>[a-z0-9-]+)$", RegexOptions.IgnoreCase)]
    private static partial Regex ProjectPathRegex();
}
