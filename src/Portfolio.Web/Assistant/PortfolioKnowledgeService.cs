using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Portfolio.Web.Assistant;

public sealed partial class PortfolioKnowledgeService : IPortfolioKnowledgeService
{
    private static readonly HashSet<string> Common = new(StringComparer.Ordinal)
    {
        "ai", "is", "his", "its", "it", "my", "you", "your", "all", "are", "of", "to", "in", "what", "which", "does", "the", "and", "can", "how", "about", "tell", "for", "this", "with",
        "من", "ما", "هل", "كيف", "عن", "هذا", "هذه", "على", "في", "هو", "هي",
        "اشرح", "المشروع", "الموجود", "الصفحه"
    };

    public Task<AssistantKnowledgeContext> GetContextAsync(string question, string language, string path,
        string section, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var culture = language == "ar" ? "ar" : "en";
        var words = Tokens(question);
        if (words.Overlaps(["passport", "password", "credentials", "secrets", "الجواز", "الاسرار"]))
            return Task.FromResult(new AssistantKnowledgeContext(culture, []));

        var catalog = BuildCatalog(culture);
        var explicitMatches = catalog
            .Where(item => item.SourceId.EndsWith("-overview", StringComparison.Ordinal)
                && Tokens(item.Title + " " + string.Join(' ', item.Keywords)).Overlaps(words))
            .Select(item => item.Url).ToHashSet(StringComparer.Ordinal);
        if (words.Overlaps(["pitch", "investor", "عرض", "مستثمر"]))
            explicitMatches.Add(culture == "ar" ? "/pitch?lang=ar" : "/pitch");
        var page = path + (culture == "ar" ? "?lang=ar" : "");
        var sections = IntentSections(words);
        if (sections.Length == 0 && section is "engineering" or "status" or "challenge" or "stack")
            sections = ["-" + section];
        var ranked = catalog
            .Select(item =>
            {
                var itemPage = item.Url.Split('#')[0];
                var topic = explicitMatches.Contains(itemPage);
                var contextual = explicitMatches.Count == 0 && path != "/" && itemPage == page;
                var termScore = Tokens(item.Content + " " + string.Join(' ', item.Keywords)).Count(words.Contains);
                var titleScore = item.Url.StartsWith("/work/", StringComparison.Ordinal)
                    ? 0 : 2 * Tokens(item.Title).Count(words.Contains);
                if (item.SourceId == "profile" && words.Overlaps(["cv", "resume", "السيره"])) titleScore += 6;
                var sectionScore = (topic || contextual) && sections.Any(s => item.SourceId.EndsWith(s, StringComparison.Ordinal)) ? 5 : 0;
                return (Item: item, Score: (topic ? 10 : contextual ? 5 : 0) + termScore + sectionScore + titleScore);
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .Select(item => item.Item)
            .ToArray();
        var topical = ranked.Where(item => explicitMatches.Contains(item.Url.Split('#')[0]))
            .DistinctBy(item => item.Url.Split('#')[0]);
        var chosen = explicitMatches.Count > 1
            ? topical.Concat(ranked).DistinctBy(item => item.SourceId).Take(5).ToArray()
            : ranked.Take(5).ToArray();
        return Task.FromResult(new AssistantKnowledgeContext(culture, chosen));
    }

    private static string[] IntentSections(HashSet<string> words)
    {
        if (words.Overlaps(["status", "roadmap", "production", "limitations", "live", "readiness", "الحاله", "حاله", "انتاج", "الانتاج", "الجاهزيه", "المستقبل", "ينفذ"]))
            return ["-status"];
        if (words.Overlaps(["architecture", "design", "work", "works", "built", "engineering", "retrieval", "معماريه", "هندسه", "يعمل", "تعمل", "استرجاع"]))
            return ["-engineering"];
        if (words.Overlaps(["stack", "technology", "technologies", "التقنيات", "تقنيات"]))
            return ["-stack"];
        if (words.Overlaps(["problem", "challenge", "المشكله", "مشكله", "التحدي", "تحدي"]))
            return ["-challenge"];
        return [];
    }

    private static AssistantKnowledgeDocument[] BuildCatalog(string culture)
    {
        var ar = culture == "ar";
        var query = ar ? "?lang=ar" : "";
        var documents = new List<AssistantKnowledgeDocument>
        {
            new("profile", ar ? "الملف المهني" : "Professional profile", "/cv" + query,
                $"{Editorial.Identity.In(culture)} — {Editorial.Profession.In(culture)}. {Editorial.Bio.In(culture)}",
                ["profile", "resume", "CV", "السيرة"]),
            new("profile-experience", ar ? "الخبرة المهنية" : "Professional experience", "/cv" + query + "#experience",
                Editorial.Experience.In(culture), ["experience", "independent", "freelance", "الخبره", "خبره"]),
            new("profile-skills", ar ? "المهارات التقنية" : "Technical skills", "/cv" + query + "#skills",
                $"{Editorial.AppliedSkills.In(culture)} {Editorial.ApplicationSkills}. {Editorial.DataSkills}.",
                ["skills", "recruiter", "job", "role", "المهارات", "مهارات", "وظيفه"]),
            new("profile-education", ar ? "التعليم واللغات" : "Education and languages", "/cv" + query + "#education",
                Editorial.Education.In(culture), ["education", "degree", "graduated", "languages", "التعليم", "تخرج", "اللغات"]),
            new("pitch", ar ? "عرض SiteAware" : "SiteAware pitch", "/pitch" + query,
                ar ? $"المستخدمون المقترحون: {Editorial.PitchAudience.In(culture)} نموذج العمل المقترح: {Editorial.PitchBusiness.In(culture)} خارطة الطريق: {Editorial.PitchRoadmap.In(culture)}" :
                    $"Proposed audiences: {Editorial.PitchAudience.In(culture)} Proposed business model: {Editorial.PitchBusiness.In(culture)} Roadmap: {Editorial.PitchRoadmap.In(culture)}",
                ["pitch", "investor", "market", "عرض", "مستثمر"]),
            new("contact", ar ? "التواصل" : "Contact", "/" + query + "#contact",
                ar ? "تتوفر روابط البريد ولينكدإن لمناقشة فرص التعاون." : "Use the public email and LinkedIn links to discuss collaboration.",
                ["contact", "email", "client", "collaboration", "تواصل", "اتواصل", "بريد", "تعاون"]),
            new("lab", ar ? "المختبر الهندسي" : "Engineering lab", "/" + query + "#lab",
                string.Join(" ", Editorial.Lab.Select(item => $"{item.Title}: {item.Description.In(culture)}")),
                ["lab", "experiments", "مختبر", "تجارب"])
        };

        foreach (var work in Editorial.Work)
        {
            var path = work.Path + query;
            documents.Add(new(work.Slug + "-overview", work.Title, path,
                $"{work.Summary.In(culture)} {work.Role.In(culture)}", work.Slug switch
                {
                    "siteaware-ai" => ["ذكائك"],
                    "wesal-mabrouk" => ["وصال", "مبروك"],
                    "level-up-pos" => ["ليفيل اب", "كاشير"],
                    _ => []
                }));
            documents.Add(new(work.Slug + "-challenge", work.Title, path + "#challenge", work.Challenge.In(culture), []));
            documents.Add(new(work.Slug + "-engineering", work.Title, path + "#engineering", work.Engineering.In(culture), []));
            documents.Add(new(work.Slug + "-status", work.Title, path + "#status", work.Status.In(culture), []));
            documents.Add(new(work.Slug + "-stack", work.Title, path + "#stack", work.Stack, []));
        }

        return documents.ToArray();
    }

    private static HashSet<string> Tokens(string text)
    {
        var normalized = new StringBuilder();
        foreach (var character in text.Normalize(NormalizationForm.FormD).ToLowerInvariant())
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            normalized.Append(character switch { 'ة' => 'ه', 'ى' => 'ي', _ => character });
        }
        return Words().Matches(normalized.ToString()).Select(match => match.Value)
            .Where(word => word.Length > 1 && !Common.Contains(word)).ToHashSet(StringComparer.Ordinal);
    }

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex Words();
}
