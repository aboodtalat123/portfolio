using Portfolio.Web;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Portfolio.Web.Components;
using Portfolio.Web.Assistant;
using Portfolio.Web.Endpoints;
using Portfolio.Core.Abstractions;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDirectory);
var connectionString = builder.Configuration.GetConnectionString("Portfolio")
    ?? $"Data Source={Path.Combine(dataDirectory, "portfolio.db")}";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddPortfolioInfrastructure(connectionString);
builder.Services.Configure<PortfolioAssistantOptions>(options =>
{
    var apiKey = builder.Configuration["Gemini:ApiKey"];
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        apiKey = builder.Configuration["GEMINI_API_KEY"];
    }
    options.ApiKey = apiKey ?? string.Empty;

    var model = builder.Configuration["Gemini:Model"];
    if (string.IsNullOrWhiteSpace(model))
    {
        model = builder.Configuration["GEN_MODEL"];
    }
    options.Model = model ?? PortfolioAssistantOptions.DefaultModel;
    options.DailyRequestLimit = Math.Clamp(builder.Configuration.GetValue("PortfolioAssistant:DailyRequestLimit", 150), 0, 1000);
});
builder.Services.AddScoped<IPortfolioKnowledgeService, PortfolioKnowledgeService>();
builder.Services.AddHttpClient<IPortfolioAssistantService, GeminiPortfolioAssistant>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(25);
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                Math.Ceiling(retry.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "rate_limited" }, cancellationToken);
    };
    options.AddPolicy(
        "portfolio-assistant",
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            GetRateLimitPartitionKey(httpContext),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseRateLimiter();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapPortfolioAssistantEndpoints();
app.MapGet("/cv/download", (IWebHostEnvironment environment) =>
{
    return Results.File(Path.Combine(environment.WebRootPath, "files", "Abdalrahman_Talat_Abu_Salem_CV.pdf"),
        "application/pdf", fileDownloadName: "Abd_Alrahman_Talat_Abu_Salem_Technical_CV.pdf", enableRangeProcessing: true);
});
app.MapGet("/pitch/download", (string? lang, IWebHostEnvironment environment) =>
{
    var fileName = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase)
        ? "SiteAware_AI_Pitch_AR.pdf" : "SiteAware_AI_Pitch_EN.pdf";
    return Results.File(Path.Combine(environment.WebRootPath, "files", fileName),
        "application/pdf", fileDownloadName: fileName, enableRangeProcessing: true);
});

app.MapGet("/api/health", (IOptions<PortfolioAssistantOptions> assistantOptions) => Results.Ok(new
{
    status = "ok",
    application = "ABD ALRAHMAN TALAT ABUSALEM Portfolio",
    framework = ".NET 10",
    assistant = string.IsNullOrWhiteSpace(assistantOptions.Value.ApiKey)
        ? "not_configured"
        : "ready"
}));

app.MapGet("/robots.txt", () =>
{
    const string origin = Editorial.PublicOrigin;
    var robots = $"""
        User-agent: *
        Allow: /

        Sitemap: {origin}/sitemap.xml
        """;

    return Results.Text(robots, "text/plain");
});

app.MapGet("/sitemap.xml", async (
    IProjectRepository projects,
    CancellationToken cancellationToken) =>
{
    const string origin = Editorial.PublicOrigin;
    var publishedProjects = await projects.GetPublishedAsync("en", cancellationToken);
    var urls = new List<string>
    {
        $"""
          <url>
            <loc>{origin}/</loc>
            <changefreq>weekly</changefreq>
            <priority>1.0</priority>
          </url>
        """,
        $"""
          <url>
            <loc>{origin}/?lang=ar</loc>
            <changefreq>weekly</changefreq>
            <priority>0.9</priority>
          </url>
        """
    };

    var paths = publishedProjects.Select(project => $"/work/{project.Slug}")
        .Concat(Editorial.Work.Select(project => project.Path))
        .Concat(["/cv", "/pitch", "/privacy"])
        .Distinct(StringComparer.Ordinal);
    urls.AddRange(paths.Select(path => $"""
          <url>
            <loc>{origin}{path}</loc>
            <changefreq>monthly</changefreq>
            <priority>0.8</priority>
          </url>
        """));

    var sitemap = $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
        {string.Join(Environment.NewLine, urls)}
        </urlset>
        """;

    return Results.Text(sitemap, "application/xml");
});

app.MapGet("/api/projects", async (
    string? lang,
    IProjectRepository projects,
    CancellationToken cancellationToken) =>
{
    var culture = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
    return Results.Ok(await projects.GetPublishedAsync(culture, cancellationToken));
});

app.MapGet("/api/projects/{slug}", async (
    string slug,
    string? lang,
    IProjectRepository projects,
    CancellationToken cancellationToken) =>
{
    var culture = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
    var project = await projects.GetBySlugAsync(slug, culture, cancellationToken);
    return project is null ? Results.NotFound() : Results.Ok(project);
});

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    await PortfolioDatabase.InitializeAsync(dbContext);
}

app.Run();

static string GetRateLimitPartitionKey(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
