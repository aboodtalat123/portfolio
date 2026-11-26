using System.Text.RegularExpressions;

namespace Portfolio.Web.Assistant;

public sealed record AssistantNavigation(string Kind, string Target, string Label);

public static partial class AssistantNavigationPolicy
{
    private static readonly (string Target, LocalText Label, string[] Aliases)[] Sections =
    [
        ("work", new("Selected work", "الأعمال المختارة"), ["work", "projects", "المشاريع", "الأعمال"]),
        ("contact", new("Contact", "التواصل"), ["contact", "التواصل"]),
        ("lab", new("Engineering lab", "المختبر الهندسي"), ["lab", "engineering lab", "المختبر"]),
        ("about", new("Engineering practice", "الممارسة الهندسية"), ["about", "profile", "الملف المهني"])
    ];

    public static AssistantNavigation? FromQuestion(ValidatedAssistantRequest request)
    {
        var command = NavigationCommand().Match(request.Question);
        if (!command.Success) return null;
        var value = command.Groups["target"].Value.Trim().TrimEnd('.', '!', '؟', '?').ToLowerInvariant();
        var query = request.Language == "ar" ? "?lang=ar" : "";

        if (value is "cv" or "the cv" or "the technical cv" or "my cv" or "resume" or "résumé" or "السيرة" or "السيرة الذاتية")
            return new("openCv", "/cv" + query, new LocalText("Open CV", "افتح السيرة").In(request.Language));

        if (value is "pitch" or "the pitch" or "siteaware pitch" or "العرض" or "عرض siteaware")
            return new("openPitch", "/pitch" + query, new LocalText("Open pitch", "افتح العرض").In(request.Language));

        var section = Sections.FirstOrDefault(item => item.Aliases.Contains(value, StringComparer.Ordinal));
        if (section.Target is not null)
            return new("scrollToSection", "/" + query + "#" + section.Target, section.Label.In(request.Language));

        var project = Editorial.Work.FirstOrDefault(item =>
            value == item.Title.ToLowerInvariant() || value == item.Slug);
        project ??= value switch
        {
            "siteaware" or "ذكائك" => Editorial.Work[0],
            "tebiha" or "تبيها" or "novixa" => Editorial.Work[1],
            "dr solution" or "دكتور سولوشن" => Editorial.Work[2],
            "level up" or "ليفيل اب" => Editorial.Work[3],
            "wesal" or "mabrouk" or "وصال" or "مبروك" => Editorial.Work[4],
            "fairaid" => Editorial.Work[5],
            _ => null
        };
        return project is null ? null : new("openProject", project.Path + query, project.Title);
    }

    [GeneratedRegex(@"^(?:open|show(?: me)?|go to|take me to|افتح|أرني|اذهب إلى)\s+(?<target>.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NavigationCommand();
}
