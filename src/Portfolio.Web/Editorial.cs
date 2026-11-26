namespace Portfolio.Web;

public sealed record LocalText(string English, string Arabic)
{
    public string In(string culture) => culture == "ar" ? Arabic : English;
}

public sealed record WorkStudy(string Slug, string Title, LocalText Role, LocalText Summary,
    LocalText Challenge, LocalText Engineering, LocalText Status, string Stack, string? Link = null)
{
    public string Path => $"/work/{Slug}";
}

public static class Editorial
{
    public const string PublicOrigin = "https://abd-alrahman-talat-abusalem-portfolio.onrender.com";

    public static readonly LocalText Identity = new("Abd Alrahman Talat Abu Salem", "عبد الرحمن طلعت أبو سالم");
    public static readonly LocalText Profession = new("Applied AI Engineer · Full-Stack Software Developer", "مهندس ذكاء اصطناعي تطبيقي · مطور برمجيات متكاملة");
    public static readonly LocalText Bio = new(
        "I build software that understands the systems people work in. My work connects grounded AI, reliable data workflows and full-stack product engineering.",
        "أبني برمجيات تفهم الأنظمة التي يعمل فيها الناس، وأجمع بين الذكاء الاصطناعي المدعوم بالأدلة وتدفقات البيانات الموثوقة وهندسة المنتجات المتكاملة.");
    public static readonly LocalText Experience = new(
        "Independent software development and applied AI engineering across product architecture, APIs, contextual interfaces and automated verification.",
        "تطوير برمجيات مستقل وهندسة ذكاء اصطناعي تطبيقي تشمل معمارية المنتجات وواجهات البرمجة والتجارب السياقية والتحقق الآلي.");
    public static readonly LocalText Education = new(
        "Bachelor of Information Technology, Mobile Computing & Smart Device Applications, Islamic University of Gaza (in progress). Arabic native, English intermediate.",
        "بكالوريوس تكنولوجيا المعلومات، الحوسبة المتنقلة وتطبيقات الأجهزة الذكية، الجامعة الإسلامية بغزة (قيد الدراسة). العربية اللغة الأم والإنجليزية مستوى متوسط.");
    public static readonly LocalText AppliedSkills = new(
        "LLM integration, hybrid retrieval, multimodal document processing, OCR and grounding evaluation.",
        "تكامل النماذج اللغوية والاسترجاع الهجين ومعالجة الوثائق متعددة الوسائط والتعرف الضوئي وتقييم الاستناد إلى الأدلة.");
    public const string ApplicationSkills = "Python · FastAPI · C# · ASP.NET Core · React · TypeScript · Blazor · PWA";
    public const string DataSkills = "PostgreSQL · pgvector · EF Core · SQLite · IndexedDB · Playwright · Docker";

    public static readonly LocalText PitchAudience = new(
        "Enterprise software, SaaS, education and public-service application teams.",
        "فرق برمجيات المؤسسات والمنصات السحابية والتعليم والخدمات العامة.");
    public static readonly LocalText PitchBusiness = new(
        "Business subscriptions and integration services.",
        "اشتراكات للشركات وخدمات تكامل.");
    public static readonly LocalText PitchRoadmap = new(
        "Complete integrated validation, improve document answer quality, validate controlled pilots and evaluate governed execution.",
        "إكمال التحقق المتكامل وتحسين جودة الإجابات المستندية والتحقق من تجارب مضبوطة وتقييم التنفيذ المحكوم.");

    public static IReadOnlyList<WorkStudy> Work { get; } =
    [
        new("siteaware-ai", "SiteAware AI",
            new("Founder & Applied AI Engineer", "المؤسس ومهندس الذكاء الاصطناعي التطبيقي"),
            new("The application intelligence layer for governed AI agents.", "طبقة ذكاء التطبيقات للوكلاء الخاضعين للضوابط."),
            new("Assistance in complex applications needs current interface context, verified routes and approved knowledge.", "تحتاج المساعدة في التطبيقات المعقدة إلى سياق الواجهة الحالية ومسارات متحققة ومعرفة معتمدة."),
            new("Authorized discovery observes the DOM and accessibility structure. Semantic learning builds an application map of pages, goals and routes. Hybrid retrieval connects approved documents with current-page context, while procedural reasoning checks evidence for navigation paths. Studio configures knowledge, the extension observes authorized interfaces, and the widget delivers contextual guidance.", "يرصد الاكتشاف المصرح به بنية الواجهة وشجرة إمكانية الوصول. يبني التعلم الدلالي خريطة للصفحات والأهداف والمسارات. يربط الاسترجاع الهجين المستندات المعتمدة بسياق الصفحة، ويتحقق الاستدلال الإجرائي من أدلة المسار. يضبط الاستوديو المعرفة، وترصد الإضافة الواجهات المصرح بها، ويقدم المساعد المدمج الإرشاد."),
            new("Guide-only today. Policy-aware planning, approval binding, preflight, independent outcome verification and audit records form the agent foundations. Live external execution is disabled and integrated validation continues.", "الوضع الحالي للإرشاد. يشمل أساس الوكلاء التخطيط وفق السياسات وربط الموافقات والتحقق المسبق والتحقق المستقل من النتائج وسجل التدقيق. التنفيذ الخارجي الحقيقي معطل والتحقق المتكامل مستمر."),
            "Python · FastAPI · React · TypeScript · PostgreSQL · pgvector"),
        new("tebiha", "TEBIHA · تبيها",
            new("Technical Product Lead & Full-Stack Developer", "قائد المنتج التقني ومطور برمجيات متكاملة"),
            new("Arabic-first Saudi B2B sourcing and procurement. وش تبي؟ نلقاه ونوصله لك", "منصة سعودية عربية للتوريد والمشتريات بين الشركات. وش تبي؟ نلقاه ونوصله لك"),
            new("Buyers need to turn incomplete requirements into comparable product and supplier candidates.", "يحتاج المشترون إلى تحويل المتطلبات الأولية إلى منتجات وموردين يمكن المقارنة بينهم."),
            new("Text, images and links become structured product intent. Multi-provider discovery normalizes and deduplicates candidates before ranking them with linked evidence. Contextual assistance supports RFQs, quotations and orders. Supplier identity resolution and landed-cost calculation have local foundations.", "تتحول النصوص والصور والروابط إلى متطلبات منظمة. يوحد البحث متعدد المصادر النتائج ويزيل التكرار قبل ترتيبها مع أدلتها. يدعم المساعد السياقي طلبات عروض الأسعار والعروض والطلبات. توجد أسس محلية لتوحيد هوية المورد وحساب التكلفة الواصلة."),
            new("Search and procurement workflows have been demonstrated in development and preview. Commercial launch, live payments and end-to-end procurement remain unverified.", "جرى عرض البحث وتدفقات المشتريات في التطوير والمعاينة. لم يُتحقق من الإطلاق التجاري أو الدفع الحي أو المشتريات من البداية إلى النهاية."),
            "Next.js · TypeScript · FastAPI · Gemini · Supabase"),
        new("dr-solution", "Dr. Solution",
            new("Full-Stack & Applied AI Developer", "مطور برمجيات متكاملة وذكاء اصطناعي تطبيقي"),
            new("A local-first academic workspace for documents, images and lectures.", "مساحة تعلم محلية أولاً للمستندات والصور والمحاضرات."),
            new("Students need understandable study material without losing the figures, tables and relationships present in the original.", "يحتاج الطلاب إلى مواد دراسية واضحة مع الحفاظ على الأشكال والجداول والعلاقات في المصدر."),
            new("OCR and layout analysis locate text regions. Translation overlays on a separate clean page preserve figures and tables where feasible, while the original remains available for comparison. Incremental preparation checkpoints completed work. A source-grounded tutor, assessment questions, spaced repetition and standalone HTML exports support review.", "يتعرف النظام على مناطق النص عبر OCR وتحليل التخطيط. تحافظ طبقات الترجمة فوق صفحة منفصلة على الرسوم والجداول حيثما أمكن، وتبقى الصفحة الأصلية متاحة للمقارنة. يحفظ التحضير التدريجي العمل المنجز عند نقاط الاستئناف، ويدعم المراجعة بمساعد مرتبط بالمصادر وأسئلة تقييم وتكرار متباعد وتصدير HTML."),
            new("The study library is local-first. Google Drive snapshots are manual; provider-dependent analysis and external lecture access depend on availability.", "مكتبة الدراسة محلية أولاً. لقطات Google Drive يدوية، ويتوقف تحليل المزود والوصول إلى المحاضرات الخارجية على التوافر."),
            "Python · FastAPI · PyMuPDF · RapidOCR · OpenCV · IndexedDB · PWA"),
        new("level-up-pos", "LEVEL UP POS",
            new("Full-Stack Developer & System Designer", "مطور برمجيات متكاملة ومصمم النظام"),
            new("An offline-first point of sale and accounting system for café operations.", "نظام نقاط بيع ومحاسبة للمقاهي يعمل محلياً أولاً."),
            new("Sales and accounting must stay usable during connectivity loss and remain auditable after synchronization.", "يجب أن يبقى البيع والمحاسبة قابلين للاستخدام عند انقطاع الاتصال وأن تظل العمليات قابلة للتدقيق بعد المزامنة."),
            new("Immutable operations feed deterministic double-entry projections. The system models sales, receivables, partial payments, inventory, wages and deposits. Multi-device synchronization and auditable corrections preserve history. The Arabic assistant requires confirmation for ledger actions.", "يغذي سجل العمليات الثابت إسقاطات حتمية للقيد المزدوج. يدير النظام البيع والذمم والدفعات الجزئية والمخزون والأجور والعربون. تحفظ المزامنة والتصحيحات القابلة للتدقيق تاريخ العمليات. يطلب المساعد العربي تأكيد إجراءات دفتر الحسابات."),
            new("Verification was conducted in staging. Production financial readiness remains unverified.", "أُجري التحقق في بيئة الاختبار المرحلي، ولا تزال الجاهزية المالية للإنتاج غير متحققة."),
            "TypeScript · PWA · Capacitor · SQLite · Cloudflare Workers / D1"),
        new("wesal-mabrouk", "Mabrouk / Wesal",
            new("AI Engineering Contributor · Team project", "مساهم في هندسة الذكاء الاصطناعي · مشروع فريق"),
            new("A contextual assistant for venue discovery and booking guidance.", "مساعد سياقي لاكتشاف القاعات والإرشاد إلى الحجز."),
            new("Venue recommendations depend on preserving budget, capacity and location across turns.", "تعتمد توصيات القاعات على الحفاظ على قيود الميزانية والسعة والموقع عبر الحوار."),
            new("Contributed to constraint-preserving recommendations, trusted retrieval, registered navigation and non-submitting booking drafts. Knowledge Studio supports reviewed knowledge and workflows for unanswered questions.", "ساهمت في توصيات تحافظ على القيود واسترجاع المعرفة الموثوقة والتنقل المسجل ومسودات الحجز دون إرسال. يدعم استوديو المعرفة مراجعة المعرفة ومعالجة الأسئلة التي لم تُجب."),
            new("Team engineering and offline evaluation. Booking assistance prepares drafts for user review.", "هندسة وتقييم محلي ضمن الفريق. تجهز المساعدة مسودات حجز يراجعها المستخدم."),
            "C# · ASP.NET Core · PostgreSQL · Next.js · Gemini"),
        new("fairaid-gaza", "FairAid Gaza",
            new("Software Developer", "مطور برمجيات"),
            new("Role-aware coordination for humanitarian registration and distribution.", "تنسيق للتسجيل والتوزيع الإنساني وفق الصلاحيات."),
            new("Teams need to coordinate the same aid types across locations and time windows while preserving operational accountability.", "تحتاج الفرق إلى تنسيق أنواع المساعدات عبر القطاعات والفترات مع الحفاظ على المساءلة التشغيلية."),
            new("Built enrollment, organization verification, distribution plans, capacity and conflict checks, delivery tracking, dashboards and audit history.", "طورت التسجيل والتحقق من المؤسسات وخطط التوزيع وفحوص السعة والتعارض ومتابعة التسليم ولوحات التشغيل وسجل التدقيق."),
            new("The code implements distribution workflows. A separate knowledge assistant remains a prototype; adoption is unverified.", "ينفذ النظام تدفقات التوزيع، ويبقى المساعد المعرفي المنفصل نموذجاً أولياً. التبني المؤسسي غير متحقق."),
            "C# · ASP.NET Core · Blazor · EF Core · SQLite / SQL Server", "https://github.com/aboodtalat123/NRCAPP")
    ];

    public static IReadOnlyList<(string Title, LocalText Description, string? Url)> Lab { get; } =
    [
        ("University AI Assistant", new("Authorized browser discovery and source-linked retrieval.", "اكتشاف مصرح به عبر المتصفح واسترجاع مرتبط بالمصادر."), null),
        ("Taqat AI Assistant", new("Course knowledge and account-scoped dashboard context.", "معرفة الدورات وسياق لوحة المستخدم ضمن حدود الحساب."), null),
        ("Video Goals Extractor", new("Timestamped frame capture, bilingual OCR and structured exports.", "التقاط الإطارات مع التوقيت والتعرف ثنائي اللغة وتصدير منظم."), "/work/video-goals-extractor"),
        ("Hesab AI Service", new("Authenticated FastAPI integration for an accounting application.", "تكامل FastAPI مصادق عليه لتطبيق محاسبي."), null)
    ];
}
