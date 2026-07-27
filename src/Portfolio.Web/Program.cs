using Portfolio.Web.Components;
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    application = "ABD ALRAHMAN TALAT ABUSALEM Portfolio",
    framework = ".NET 10"
}));

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
