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
builder.Services.AddMemoryCache();
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

app.MapGet("/api/health", (IOptions<PortfolioAssistantOptions> assistantOptions) => Results.Ok(new
{
    status = "ok",
    application = "ABD ALRAHMAN TALAT ABUSALEM Portfolio",
    framework = ".NET 10",
    assistant = string.IsNullOrWhiteSpace(assistantOptions.Value.ApiKey)
        ? "not_configured"
        : "ready"
}));

app.MapGet("/robots.txt", (HttpRequest request) =>
{
    var origin = GetPublicOrigin(request);
    var robots = $"""
        User-agent: *
        Allow: /

        Sitemap: {origin}/sitemap.xml
        """;

    return Results.Text(robots, "text/plain");
});

app.MapGet("/sitemap.xml", async (
    HttpRequest request,
    IProjectRepository projects,
    CancellationToken cancellationToken) =>
{
    var origin = GetPublicOrigin(request);
    var lastModified = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
    var publishedProjects = await projects.GetPublishedAsync("en", cancellationToken);
    var urls = new List<string>
    {
        $"""
          <url>
            <loc>{origin}/</loc>
            <lastmod>{lastModified}</lastmod>
            <changefreq>weekly</changefreq>
            <priority>1.0</priority>
          </url>
        """,
        $"""
          <url>
            <loc>{origin}/?lang=ar</loc>
            <lastmod>{lastModified}</lastmod>
            <changefreq>weekly</changefreq>
            <priority>0.9</priority>
          </url>
        """
    };

    urls.AddRange(publishedProjects.Select(project => $"""
          <url>
            <loc>{origin}/work/{project.Slug}</loc>
            <lastmod>{lastModified}</lastmod>
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

static string GetPublicOrigin(HttpRequest request)
{
    var forwardedScheme = request.Headers["X-Forwarded-Proto"]
        .ToString()
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .FirstOrDefault();

    var scheme = forwardedScheme is not null
        && (forwardedScheme.Equals("https", StringComparison.OrdinalIgnoreCase)
            || forwardedScheme.Equals("http", StringComparison.OrdinalIgnoreCase))
            ? forwardedScheme.ToLowerInvariant()
            : request.Scheme;

    return $"{scheme}://{request.Host}";
}

static string GetRateLimitPartitionKey(HttpContext context)
{
    var forwardedAddress = context.Request.Headers["X-Forwarded-For"]
        .ToString()
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .FirstOrDefault();

    return forwardedAddress
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";
}
