using Microsoft.EntityFrameworkCore;
using Portfolio.Core.Entities;

namespace Portfolio.Infrastructure.Persistence;

public static class PortfolioDatabase
{
    private sealed record TechnologySeed(string Name, string Slug, string Category);

    private sealed record TranslationSeed(
        string Culture,
        string Title,
        string Category,
        string Summary,
        string Challenge,
        string Solution,
        string Role,
        string Outcome);

    private sealed record MediaSeed(string Path, string AltEn, string AltAr);

    private sealed record ProjectSeed(
        Guid Id,
        string Slug,
        bool IsFeatured,
        int SortOrder,
        int CompletedYear,
        string? PreviewUrl,
        string? RepositoryUrl,
        IReadOnlyList<TranslationSeed> Translations,
        IReadOnlyList<string> TechnologySlugs,
        MediaSeed Media);

    public static async Task InitializeAsync(
        PortfolioDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var technologies = await UpsertTechnologiesAsync(dbContext, cancellationToken);
        await UpsertProjectsAsync(dbContext, technologies, cancellationToken);
    }

    private static async Task<IReadOnlyDictionary<string, Technology>> UpsertTechnologiesAsync(
        PortfolioDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var existing = (await dbContext.Technologies.ToListAsync(cancellationToken))
            .ToDictionary(item => item.Slug, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in TechnologySeeds())
        {
            if (!existing.TryGetValue(seed.Slug, out var technology))
            {
                technology = new Technology
                {
                    Id = Guid.NewGuid(),
                    Slug = seed.Slug
                };
                dbContext.Technologies.Add(technology);
                existing[seed.Slug] = technology;
            }

            technology.Name = seed.Name;
            technology.Category = seed.Category;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static async Task UpsertProjectsAsync(
        PortfolioDbContext dbContext,
        IReadOnlyDictionary<string, Technology> technologies,
        CancellationToken cancellationToken)
    {
        var projects = (await dbContext.Projects
                .Include(project => project.Translations)
                .Include(project => project.Media)
                .Include(project => project.Technologies)
                    .ThenInclude(link => link.Technology)
                .AsSplitQuery()
                .ToListAsync(cancellationToken))
            .ToDictionary(project => project.Slug, StringComparer.OrdinalIgnoreCase);

        foreach (var seed in ProjectSeeds())
        {
            if (!projects.TryGetValue(seed.Slug, out var project))
            {
                project = new Project
                {
                    Id = seed.Id,
                    Slug = seed.Slug,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                dbContext.Projects.Add(project);
                projects[seed.Slug] = project;
            }

            project.Status = ProjectStatus.Published;
            project.IsFeatured = seed.IsFeatured;
            project.SortOrder = seed.SortOrder;
            project.CompletedYear = seed.CompletedYear;
            project.PreviewUrl = seed.PreviewUrl;
            project.RepositoryUrl = seed.RepositoryUrl;
            project.UpdatedAt = DateTimeOffset.UtcNow;

            UpsertTranslations(dbContext, project, seed.Translations);
            UpsertMedia(dbContext, project, seed.Media);
            SyncTechnologies(dbContext, project, seed.TechnologySlugs, technologies);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void UpsertTranslations(
        PortfolioDbContext dbContext,
        Project project,
        IReadOnlyList<TranslationSeed> translations)
    {
        foreach (var seed in translations)
        {
            var translation = project.Translations.FirstOrDefault(item => item.Culture == seed.Culture);
            if (translation is null)
            {
                translation = new ProjectTranslation
                {
                    ProjectId = project.Id,
                    Project = project,
                    Culture = seed.Culture
                };
                dbContext.ProjectTranslations.Add(translation);
            }

            translation.Title = seed.Title;
            translation.Category = seed.Category;
            translation.Summary = seed.Summary;
            translation.Challenge = seed.Challenge;
            translation.Solution = seed.Solution;
            translation.Role = seed.Role;
            translation.Outcome = seed.Outcome;
        }
    }

    private static void UpsertMedia(
        PortfolioDbContext dbContext,
        Project project,
        MediaSeed seed)
    {
        var media = project.Media
            .OrderByDescending(item => item.IsThumbnail)
            .ThenBy(item => item.SortOrder)
            .FirstOrDefault();

        if (media is null)
        {
            media = new ProjectMedia
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Project = project
            };
            dbContext.ProjectMedia.Add(media);
        }

        media.FileName = seed.Path;
        media.AltTextEn = seed.AltEn;
        media.AltTextAr = seed.AltAr;
        media.IsThumbnail = true;
        media.SortOrder = 1;
    }

    private static void SyncTechnologies(
        PortfolioDbContext dbContext,
        Project project,
        IReadOnlyList<string> desiredSlugs,
        IReadOnlyDictionary<string, Technology> technologies)
    {
        var desired = desiredSlugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var linksToRemove = project.Technologies
            .Where(link => !desired.Contains(link.Technology.Slug))
            .ToArray();

        if (linksToRemove.Length > 0)
        {
            dbContext.ProjectTechnologies.RemoveRange(linksToRemove);
        }

        var current = project.Technologies
            .Where(link => desired.Contains(link.Technology.Slug))
            .Select(link => link.Technology.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var slug in desired.Where(slug => !current.Contains(slug)))
        {
            var technology = technologies[slug];
            dbContext.ProjectTechnologies.Add(new ProjectTechnology
            {
                ProjectId = project.Id,
                TechnologyId = technology.Id,
                Project = project,
                Technology = technology
            });
        }
    }

    private static TechnologySeed[] TechnologySeeds() =>
    [
        new("ASP.NET Core", "aspnet-core", "Backend"),
        new("Blazor", "blazor", "Frontend"),
        new("Entity Framework Core", "ef-core", "Data"),
        new("SQL Server", "sql-server", "Data"),
        new("SQLite", "sqlite", "Data"),
        new("FastAPI", "fastapi", "Backend"),
        new("Python", "python", "Backend"),
        new("Gemini", "gemini", "AI"),
        new("ChromaDB", "chromadb", "AI"),
        new("RAG", "rag", "AI"),
        new("JavaScript", "javascript", "Frontend"),
        new("Chrome Extension", "chrome-extension", "Frontend"),
        new("Docker", "docker", "Delivery"),
        new("PyMuPDF", "pymupdf", "Documents"),
        new("Pydantic", "pydantic", "Backend"),
        new("IndexedDB", "indexeddb", "Data"),
        new("FFmpeg", "ffmpeg", "Media"),
        new("Tesseract OCR", "tesseract", "AI"),
        new("Claude Vision", "claude-vision", "AI"),
        new("yt-dlp", "yt-dlp", "Media")
    ];

    private static ProjectSeed[] ProjectSeeds() =>
    [
        new(
            Guid.Parse("b4d652a0-25df-49d1-b148-677d060754b2"),
            "fairaid-gaza",
            true,
            3,
            2026,
            "https://nrcapp.onrender.com",
            "https://github.com/aboodtalat123/NRCAPP",
            [
                new(
                    "en",
                    "FairAid Gaza",
                    "Aid Operations Platform",
                    "An Arabic-first coordination platform with role-aware workflows for organizations, citizens, and administrators—covering distribution planning, conflict checks, enrollment, delivery tracking, analytics, and grounded AI assistance.",
                    "Aid operations need one reliable workflow without duplicate plans, overbooked distributions, unclear ownership, or disconnected follow-up.",
                    "A secure ASP.NET Core and Blazor platform with separate role journeys, validated forms, a 48-hour plan conflict window, capacity-aware citizen enrollment, attendance and delivery states, audit logs, and operational analytics.",
                    "Product architecture, data modeling, backend services, Blazor interface, authentication, validation, testing, deployment, and AI integration.",
                    "A working end-to-end product that demonstrates coordinated planning and transparent follow-up from the organization dashboard to the citizen timeline."),
                new(
                    "ar",
                    "منصة FairAid Gaza",
                    "منصة عمليات المساعدات",
                    "منصة عربية لإدارة التنسيق بين المؤسسات والمواطنين والإدارة، تشمل خطط التوزيع ومنع التعارض والتسجيل ومتابعة التسليم والتحليلات ومساعدًا ذكيًا موثّقًا.",
                    "تحتاج عمليات المساعدات إلى مسار موحد يمنع تكرار الخطط وتجاوز السعة ويجعل المسؤوليات والمتابعة واضحة.",
                    "منصة آمنة مبنية بـASP.NET Core وBlazor، مع رحلات منفصلة حسب الدور، ونماذج موثقة، وفحص تعارض خلال 48 ساعة، وتسجيل حسب السعة، وحالات حضور وتسليم، وسجل تدقيق وتحليلات تشغيلية.",
                    "معمارية المنتج ونمذجة البيانات وخدمات الخادم وواجهة Blazor والمصادقة والتحقق والاختبارات والنشر ودمج الذكاء الاصطناعي.",
                    "منتج متكامل يوضح التنسيق من لوحة المؤسسة حتى الجدول الزمني للمواطن ضمن واجهة عربية واحدة.")
            ],
            ["aspnet-core", "blazor", "ef-core", "sql-server", "sqlite", "fastapi", "gemini", "docker"],
            new(
                "/media/projects/fairaid-cover.png",
                "FairAid Gaza Arabic operations dashboard",
                "لوحة عمليات منصة FairAid Gaza العربية")),
        new(
            Guid.Parse("3f52e32f-9067-4af5-a699-c37f4971262e"),
            "reusable-site-ai",
            true,
            1,
            2026,
            null,
            null,
            [
                new(
                    "en",
                    "Safe Site-Aware AI Layer",
                    "Reusable AI Integration System",
                    "I designed and built a reusable AI layer that can be adapted to existing websites without direct database access. It combines site-wide knowledge with the user's live current page, answers from accessible content, cites exact source pages, and can take the user to the relevant route.",
                    "Large websites scatter useful information across many routes. Generic chatbots do not know what the user is currently seeing, cannot reliably cite the right page, and often require risky or unrealistic access to internal systems.",
                    "A Chrome Manifest V3 extension and FastAPI service read the visible page at question time, retrieve supporting knowledge from sanitized site content, prioritize live context, and return grounded answers with exact-page navigation. SQLite and ChromaDB keep each website and user workspace isolated.",
                    "Product concept, system architecture, site adapters, safe discovery, URL canonicalization, retrieval and grounding, Chrome extension UX, automated tests, and deployment.",
                    "The same core architecture was proven on an authenticated Oracle APEX university portal and Taqat Academy. Both integrations work from user-accessible content without collecting credentials or connecting the assistant directly to either website database."),
                new(
                    "ar",
                    "طبقة ذكاء آمنة ومدركة للموقع",
                    "نظام دمج ذكاء اصطناعي قابل لإعادة الاستخدام",
                    "صممت وطورت طبقة ذكاء قابلة للتكييف مع المواقع القائمة دون وصول مباشر إلى قواعد بياناتها. تجمع معرفة الموقع مع الصفحة المفتوحة للمستخدم، وتجيب من المحتوى المتاح، وتوثق المصدر الدقيق، وتوجه المستخدم إلى الصفحة المناسبة.",
                    "تتوزع المعلومات داخل المواقع الكبيرة بين مسارات كثيرة، بينما لا تعرف روبوتات الدردشة العامة ما يراه المستخدم الآن ولا تستطيع دائماً توثيق الصفحة الصحيحة، وقد تتطلب صلاحيات داخلية غير مناسبة.",
                    "إضافة Chrome Manifest V3 وخدمة FastAPI تقرآن الصفحة الظاهرة وقت السؤال، وتسترجعان الأدلة من محتوى الموقع المنظف، وتعطيان أولوية للسياق الحي، ثم تعيدان إجابة موثقة مع انتقال للصفحة الدقيقة. تعزل SQLite وChromaDB كل موقع ومساحة مستخدم.",
                    "فكرة المنتج ومعمارية النظام ومحولات المواقع والاستكشاف الآمن وتوحيد الروابط والاسترجاع والتوثيق وتجربة الإضافة والاختبارات والنشر.",
                    "تم إثبات النواة نفسها على بوابة جامعة موثقة مبنية بـOracle APEX ومنصة طاقات، دون جمع كلمات المرور أو ربط المساعد مباشرة بقاعدة بيانات أي موقع.")
            ],
            ["fastapi", "gemini", "chromadb", "javascript", "chrome-extension", "sqlite", "rag", "docker"],
            new(
                "/media/projects/site-assistant-cover.png",
                "University portal knowledge assistant showing indexed coverage",
                "مساعد بوابة الجامعة يعرض الصفحات المفهرسة ونسبة التغطية")),
        new(
            Guid.Parse("71f0db44-f94b-4e76-bbd8-f482833bc4ef"),
            "dr-solution",
            true,
            2,
            2026,
            "https://dr-solution.onrender.com",
            "https://github.com/aboodtalat123/dr-solution",
            [
                new(
                    "en",
                    "Dr. Solution",
                    "Live AI Study Product",
                    "A deployed Arabic-first study product that converts PDF files and images into visual slides with translation, explanation, image analysis, summaries, configurable quizzes, contextual chat, local history, and standalone HTML export.",
                    "Academic documents mix text, tables, diagrams, and scanned pages; a plain text summary loses the visual context students need to understand them.",
                    "A FastAPI document pipeline renders each page with PyMuPDF, sends text and page imagery through multimodal Gemini analysis, and builds a responsive study interface with local history and an interactive offline export.",
                    "Product design, FastAPI API, document extraction, AI orchestration, Arabic frontend, IndexedDB history, HTML exporter, automated tests, Docker, and deployment setup.",
                    "A live version 3.0.0 BYOK product on Render, backed by eight passing automated tests, that turns documents into a structured learning experience while keeping the user's API key out of stored history and exported files."),
                new(
                    "ar",
                    "Dr. Solution",
                    "منتج تعلم حي بالذكاء الاصطناعي",
                    "منتج عربي منشور يحول ملفات PDF والصور إلى سلايدات بصرية مع ترجمة وشرح وتحليل صور وخلاصات واختبارات قابلة للتخصيص ومحادثة وسجل محلي وتصدير HTML مستقل.",
                    "تجمع المستندات الأكاديمية بين النصوص والجداول والرسوم والصور الممسوحة، بينما تفقد الخلاصة النصية وحدها السياق البصري المهم للفهم.",
                    "مسار معالجة بـFastAPI يرسم كل صفحة عبر PyMuPDF، ويرسل النص والصورة إلى تحليل Gemini متعدد الوسائط، ثم يبني تجربة دراسة متجاوبة مع سجل محلي وتصدير تفاعلي يعمل دون خادم.",
                    "تصميم المنتج وواجهة FastAPI واستخراج المستندات وتنسيق الذكاء والواجهة العربية وIndexedDB ومصدر HTML والاختبارات وDocker وتجهيز النشر.",
                    "منتج BYOK حي بإصدار 3.0.0 على Render ومدعوم بثمانية اختبارات ناجحة، يحول المستند إلى تجربة تعلم منظمة دون حفظ مفتاح المستخدم في السجل أو الملف المصدر.")
            ],
            ["python", "fastapi", "gemini", "pymupdf", "pydantic", "javascript", "indexeddb", "docker"],
            new(
                "/media/projects/dr-solution-cover.png",
                "Dr. Solution document analysis workspace",
                "واجهة تحليل المستندات في Dr. Solution")),
        new(
            Guid.Parse("04526f1a-0218-475f-b989-72a10d6f5e41"),
            "video-goals-extractor",
            false,
            4,
            2026,
            null,
            "https://github.com/aboodtalat123/video-goals-extractor",
            [
                new(
                    "en",
                    "Video Goals Extractor",
                    "AI Media Automation Lab",
                    "A local web app and CLI that processes YouTube videos and playlists, extracts slide frames, reads Arabic and English learning goals with OCR or vision AI, and exports structured reports.",
                    "Course videos hide structured learning goals inside transient slides that are slow to review manually across long playlists.",
                    "A media pipeline downloads permitted inputs, samples frames, detects useful slides, applies OCR or vision analysis, deduplicates results, and exports a reviewable report.",
                    "Pipeline design, local web interface, CLI workflow, media processing, OCR and vision integration, and structured export.",
                    "A focused automation tool that converts long-form educational video material into a concise, reviewable set of learning goals."),
                new(
                    "ar",
                    "أداة استخراج أهداف الفيديو",
                    "مختبر أتمتة وسائط بالذكاء الاصطناعي",
                    "تطبيق ويب محلي وأداة أوامر لمعالجة فيديوهات وقوائم YouTube واستخراج إطارات الشرائح وقراءة الأهداف العربية والإنجليزية عبر OCR أو الذكاء البصري وتصدير تقارير منظمة.",
                    "تُخفى أهداف التعلم داخل شرائح سريعة الظهور في فيديوهات طويلة، ما يجعل مراجعتها يدويًا بطيئة خصوصًا عبر قوائم كاملة.",
                    "مسار وسائط يجلب المدخلات المسموحة ويأخذ عينات من الإطارات ويكشف الشرائح المفيدة ويطبق OCR أو التحليل البصري ثم يزيل التكرار ويصدر تقريرًا قابلًا للمراجعة.",
                    "تصميم المسار وواجهة الويب المحلية وأداة الأوامر ومعالجة الوسائط ودمج OCR والذكاء البصري والتصدير المنظم.",
                    "أداة مركزة تحول المادة التعليمية المرئية الطويلة إلى مجموعة مختصرة وقابلة للمراجعة من أهداف التعلم.")
            ],
            ["python", "fastapi", "ffmpeg", "tesseract", "claude-vision", "yt-dlp"],
            new(
                "/media/projects/video-goals-cover.png",
                "Video Goals Extractor local web interface",
                "واجهة أداة استخراج أهداف الفيديو المحلية"))
    ];
}
