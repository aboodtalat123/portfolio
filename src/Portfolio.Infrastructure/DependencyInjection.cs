using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Core.Abstractions;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Repositories;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPortfolioInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<PortfolioDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IProjectRepository, ProjectRepository>();
        return services;
    }
}
