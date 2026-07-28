using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Portfolio.Core.Abstractions;
using Portfolio.Core.Models;

namespace Portfolio.Web.Assistant;

public sealed partial class PortfolioKnowledgeService(
    IProjectRepository projects,
    IMemoryCache cache) : IPortfolioKnowledgeService
{
    private const int MaximumContextDocuments = 4;

    public async Task<AssistantKnowledgeContext> GetContextAsync(
        string question,
        string language,
        string path,
        string section,
        CancellationToken cancellationToken = default)
    {
        var culture = string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        var catalog = await GetCatalogAsync(culture, cancellationToken);
        var preferredIds = PreferredSourceIds(path, section);
        var queryTokens = Tokenize(question);

        var ranked = catalog
            .Select((document, index) => new
            {
                Document = document,
                Index = index,
                Score = Score(document, queryTokens)
                    + (preferredIds.Contains(document.SourceId) ? 100 : 0)
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Index)
            .ToArray();

        var selected = ranked
            .Where(item => item.Score > 0)
            .Take(MaximumContextDocuments)
            .Select(item => Localize(item.Document, culture))
            .ToList();

        if (selected.Count == 0)
        {
            selected.AddRange(catalog
                .Where(document => document.SourceId is "profile" or "site-ai")
                .Take(MaximumContextDocuments)
                .Select(document => Localize(document, culture)));
        }

        if (preferredIds.Count > 0)
        {
            foreach (var preferredId in preferredIds.Reverse())
            {
                var preferred = catalog.FirstOrDefault(document => document.SourceId == preferredId);
                if (preferred is null)
                {
                    continue;
                }

                selected.RemoveAll(document => document.SourceId == preferredId);
                selected.Insert(0, Localize(preferred, culture));
            }

            if (selected.Count > MaximumContextDocuments)
            {
                selected.RemoveRange(MaximumContextDocuments, selected.Count - MaximumContextDocuments);
            }
        }

        return new AssistantKnowledgeContext(culture, selected);
    }

    private async Task<IReadOnlyList<AssistantKnowledgeDocument>> GetCatalogAsync(
        string culture,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"portfolio-assistant-catalog:{culture}";
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<AssistantKnowledgeDocument>? cached)
            && cached is not null)
        {
            return cached;
        }

        var published = await projects.GetPublishedAsync(culture, cancellationToken);
        var featured = published
            .Where(project => project.IsFeatured)
            .ToDictionary(project => project.Slug, StringComparer.OrdinalIgnoreCase);

        var details = new Dictionary<string, ProjectDetail>(StringComparer.OrdinalIgnoreCase);
        foreach (var project in featured.Values)
        {
            var detail = await projects.GetBySlugAsync(project.Slug, culture, cancellationToken);
            if (detail is not null)
            {
                details[project.Slug] = detail;
            }
        }

        var catalog = culture == "ar"
            ? BuildArabicCatalog(details)
            : BuildEnglishCatalog(details);

        cache.Set(cacheKey, catalog, TimeSpan.FromHours(12));
        return catalog;
    }

    private static IReadOnlyList<AssistantKnowledgeDocument> BuildEnglishCatalog(
        IReadOnlyDictionary<string, ProjectDetail> details)
    {
        var documents = new List<AssistantKnowledgeDocument>
        {
            new(
                "profile",
                "ABD ALRAHMAN TALAT ABUSALEM — Profile",
                "/#about",
                """
                ABD ALRAHMAN TALAT ABUSALEM, also known online as aboodtalat and aboodtalat123, is a Gaza-based .NET Backend and Applied AI Engineer available for selected remote work.
                His primary stack is C#, ASP.NET Core, Blazor, Entity Framework Core, SQL Server, and SQLite. His applied-AI work uses FastAPI, Gemini, retrieval-augmented generation, ChromaDB, and JavaScript.
                He is completing a Bachelor of Information Technology in Mobile Computing and Smart Devices at the Islamic University of Gaza, with graduation expected in September 2026.
                Training includes Full Stack with ASP.NET Core at Areisto Software Company, Khireej Qader Season 5 (90 hours, May 2026), Cyber Security at Luminus Technical University College (334 hours, November 2025), and Artificial Intelligence Training through Taqat × Code Map (2026).
                Experience includes Technology Club Operations Manager at the Islamic University of Gaza (2022–2026) and freelance English-Arabic translation and subtitle work for Oxfam (2024–present).
                He works in Arabic and English and focuses on secure backend systems, complete web products, and practical AI integration.
                """,
                [
                    "abd alrahman", "abusalem", "aboodtalat", "profile", "about", "skills",
                    "experience", "education", "training", "asp.net", "c#", "backend", "full stack",
                    "arabic", "english", "gaza", "oxfam", "technology club"
                ]),
            new(
                "contact",
                "Contact and professional profiles",
                "/#contact",
                """
                Contact ABD ALRAHMAN TALAT ABUSALEM by email at aboodtalat123@gmail.com.
                GitHub: https://github.com/aboodtalat123
                LinkedIn: https://www.linkedin.com/in/abd-alrahman-talat-abusalem-1115a23a7/
                His current CV is available from the Resume link on this portfolio.
                """,
                ["contact", "email", "hire", "work", "github", "linkedin", "resume", "cv", "message"])
        };

        AddProjectDocuments(documents, details, "en");
        return documents;
    }

    private static IReadOnlyList<AssistantKnowledgeDocument> BuildArabicCatalog(
        IReadOnlyDictionary<string, ProjectDetail> details)
    {
        var documents = new List<AssistantKnowledgeDocument>
        {
            new(
                "profile",
                "الملف المهني لعبد الرحمن طلعت أبو سالم",
                "/#about",
                """
                عبد الرحمن طلعت أبو سالم، ويُعرف رقميًا أيضًا باسم aboodtalat وaboodtalat123، مهندس Backend بـ.NET ومطور حلول ذكاء اصطناعي تطبيقي من غزة، ومتاح لأعمال مختارة عن بُعد.
                تقنياته الأساسية هي C# وASP.NET Core وBlazor وEntity Framework Core وSQL Server وSQLite. ويستخدم في حلول الذكاء التطبيقي FastAPI وGemini وRAG وChromaDB وJavaScript.
                يدرس بكالوريوس تكنولوجيا المعلومات في تخصص الحوسبة المتنقلة والأجهزة الذكية في الجامعة الإسلامية بغزة، ومن المتوقع تخرجه في سبتمبر 2026.
                تشمل تدريباته: Full Stack باستخدام ASP.NET Core لدى Areisto Software Company ضمن Khireej Qader Season 5 بواقع 90 ساعة في مايو 2026، والأمن السيبراني في Luminus Technical University College بواقع 334 ساعة في نوفمبر 2025، وتدريب الذكاء الاصطناعي مع طاقات وCode Map عام 2026.
                تشمل خبرته إدارة عمليات نادي التكنولوجيا في الجامعة الإسلامية بغزة بين 2022 و2026، والعمل الحر في الترجمة والترجمة النصية بين الإنجليزية والعربية مع Oxfam منذ 2024.
                يعمل بالعربية والإنجليزية، ويركز على أنظمة Backend الآمنة ومنتجات الويب المتكاملة ودمج الذكاء الاصطناعي عمليًا.
                """,
                [
                    "عبد الرحمن", "ابو سالم", "عبود طلعت", "الملف", "عنه", "المهارات",
                    "الخبرة", "التعليم", "التدريب", "دوت نت", "باك اند", "فول ستاك",
                    "غزة", "اوكسفام", "نادي التكنولوجيا"
                ]),
            new(
                "contact",
                "التواصل والحسابات المهنية",
                "/#contact",
                """
                يمكن التواصل مع عبد الرحمن طلعت أبو سالم عبر البريد: aboodtalat123@gmail.com
                GitHub: https://github.com/aboodtalat123
                LinkedIn: https://www.linkedin.com/in/abd-alrahman-talat-abusalem-1115a23a7/
                يمكن فتح السيرة الذاتية الحالية من رابط السيرة الذاتية في هذا الموقع.
                """,
                ["تواصل", "البريد", "توظيف", "عمل", "جيت هاب", "لينكد ان", "السيرة", "رسالة"])
        };

        AddProjectDocuments(documents, details, "ar");
        return documents;
    }

    private static void AddProjectDocuments(
        ICollection<AssistantKnowledgeDocument> documents,
        IReadOnlyDictionary<string, ProjectDetail> details,
        string culture)
    {
        if (details.TryGetValue("reusable-site-ai", out var siteAi))
        {
            var adapters = culture == "ar"
                ? """
                  تم إثبات النواة نفسها من خلال تطبيقين فعليين:
                  1. بوابة الجامعة المبنية بـOracle APEX: يقرأ المساعد ما يستطيع الطالب المسجل رؤيته، يعطي الصفحة الحالية الأولوية، ويعرض مصدرًا دقيقًا دون جمع بيانات الدخول أو الاتصال بقاعدة بيانات الجامعة.
                  2. Taqat Academy: تم تكييف النواة نفسها مع تصميم وبنية معرفة مختلفين لإثبات أنها قابلة لإعادة الاستخدام وليست مرتبطة ببوابة واحدة.
                  التنفيذان خاصان والمعاينة متاحة عند الطلب.
                  """
                : """
                  The same core was proven through two working integrations:
                  1. An authenticated Oracle APEX university portal: the assistant reads only what the signed-in student can already see, prioritizes the current page, and exposes an exact source without collecting credentials or connecting to the university database.
                  2. Taqat Academy: the same isolated core was adapted to a different design and knowledge structure, proving that it is reusable rather than tied to one portal.
                  Both implementations are private and access is available on request.
                  """;

            documents.Add(ProjectDocument(
                "site-ai",
                siteAi,
                "/work/reusable-site-ai",
                adapters,
                [
                    "site aware ai", "website assistant", "university", "oracle apex", "taqat academy",
                    "current page", "live context", "rag", "sources", "no database", "privacy",
                    "مساعد المواقع", "الجامعة", "طاقات", "الصفحة الحالية", "مصادر", "بدون قاعدة بيانات"
                ]));
        }

        if (details.TryGetValue("dr-solution", out var drSolution))
        {
            documents.Add(ProjectDocument(
                "dr-solution",
                drSolution,
                "/work/dr-solution",
                string.Empty,
                [
                    "dr solution", "pdf", "images", "study", "slides", "translation", "quiz",
                    "gemini", "pymupdf", "fastapi", "تعليم", "دراسة", "ملفات", "شرح", "اختبارات"
                ]));
        }

        if (details.TryGetValue("fairaid-gaza", out var fairAid))
        {
            documents.Add(ProjectDocument(
                "fairaid",
                fairAid,
                "/work/fairaid-gaza",
                string.Empty,
                [
                    "fairaid", "nrcapp", "aid", "humanitarian", "blazor", "asp.net",
                    "coordination", "distribution", "analytics", "مساعدات", "اغاثة", "توزيع", "تنسيق"
                ]));
        }
    }

    private static AssistantKnowledgeDocument ProjectDocument(
        string sourceId,
        ProjectDetail project,
        string url,
        string extra,
        IReadOnlyList<string> keywords)
    {
        var content = $"""
            Project: {project.Title}
            Category: {project.Category}
            Summary: {project.Summary}
            Challenge: {project.Challenge}
            Solution: {project.Solution}
            Abd Alrahman's role: {project.Role}
            Outcome: {project.Outcome}
            Technologies: {string.Join(", ", project.Technologies)}
            Year: {project.CompletedYear}
            Live product: {project.PreviewUrl ?? "Private or not publicly available"}
            Repository: {project.RepositoryUrl ?? "Private or not publicly available"}
            {extra}
            """;

        return new AssistantKnowledgeDocument(sourceId, project.Title, url, content, keywords);
    }

    private static HashSet<string> PreferredSourceIds(string path, string section)
    {
        if (path.StartsWith("/work/", StringComparison.OrdinalIgnoreCase))
        {
            return path["/work/".Length..] switch
            {
                "reusable-site-ai" => ["site-ai"],
                "dr-solution" => ["dr-solution"],
                "fairaid-gaza" => ["fairaid"],
                _ => []
            };
        }

        return section switch
        {
            "system" => ["site-ai"],
            "work" => ["site-ai", "dr-solution", "fairaid"],
            "about" => ["profile"],
            "contact" => ["contact"],
            _ => []
        };
    }

    private static AssistantKnowledgeDocument Localize(
        AssistantKnowledgeDocument document,
        string culture) =>
        document with { Url = LocalizeUrl(document.Url, culture) };

    private static string LocalizeUrl(string url, string culture)
    {
        if (culture != "ar")
        {
            return url;
        }

        var fragmentIndex = url.IndexOf('#');
        var fragment = fragmentIndex >= 0 ? url[fragmentIndex..] : string.Empty;
        var path = fragmentIndex >= 0 ? url[..fragmentIndex] : url;
        var separator = path.Contains('?') ? '&' : '?';
        return $"{path}{separator}lang=ar{fragment}";
    }

    private static int Score(AssistantKnowledgeDocument document, HashSet<string> queryTokens)
    {
        if (queryTokens.Count == 0)
        {
            return 0;
        }

        var titleTokens = Tokenize(document.Title);
        var keywordTokens = document.Keywords.SelectMany(Tokenize).ToHashSet(StringComparer.Ordinal);
        var contentTokens = Tokenize(document.Content);
        var score = 0;

        foreach (var token in queryTokens)
        {
            if (titleTokens.Contains(token))
            {
                score += 5;
            }

            if (keywordTokens.Contains(token))
            {
                score += 3;
            }

            if (contentTokens.Contains(token))
            {
                score += 1;
            }
        }

        return score;
    }

    private static HashSet<string> Tokenize(string value)
    {
        var normalized = NormalizeSearchText(value);
        return TokenRegex()
            .Matches(normalized)
            .Select(match => match.Value)
            .Where(token => token.Length >= 2)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string NormalizeSearchText(string value)
    {
        var decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(character switch
            {
                'أ' or 'إ' or 'آ' => 'ا',
                'ى' => 'ي',
                'ة' => 'ه',
                'ؤ' => 'و',
                'ئ' => 'ي',
                _ => character
            });
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    [GeneratedRegex(@"[\p{L}\p{N}#+.]+")]
    private static partial Regex TokenRegex();
}
